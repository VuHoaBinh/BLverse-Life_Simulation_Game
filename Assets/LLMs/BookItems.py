from typing import Optional
from transformers import AutoTokenizer,AutoModelForCausalLM
import re
from datasets import Dataset
import os

# Constants
BASE_MODEL = "meta-llama/Meta-Llama-3.1-8B"
MIN_TOKENS = 150  # Minimum tokens for useful content
MAX_TOKENS = 160  # Maximum tokens before truncation
MIN_CHARS = 300   # Minimum characters
CEILING_CHARS = MAX_TOKENS * 7  # Approximate ceiling for characters

class BookItem:
    """
    A BookItem represents a cleaned book entry with its price for price prediction
    """
    
    tokenizer = AutoTokenizer.from_pretrained(BASE_MODEL, trust_remote_code=True)
    PREFIX = "Price is $"
    QUESTION = "How much does this book cost to the nearest dollar?"
    
   
    title: str
    price: float
    category: str
    token_count: int = 0
    details: Optional[str]
    prompt: Optional[str] = None
    include = False
    
    def __init__(self, data, price):
        self.title = data['title']
        self.price = price
        self.category = data.get('main_category', '')
        self.parse(data)
    
    
    def clean_text(self, stuff):
        """
        Clean up text by removing unnecessary characters and whitespace
        Also remove likely product IDs (words 7+ chars with numbers)
        """
        if not stuff:
            return ""
        
        # Convert to string if it's not already
        if not isinstance(stuff, str):
            stuff = str(stuff)
            
        # Clean up special characters and extra whitespace
        stuff = re.sub(r'[:\[\]"{}【】\s]+', ' ', stuff).strip()
        stuff = stuff.replace(" ,", ",").replace(",,,", ",").replace(",,", ",")
        
        # Remove likely product IDs
        words = stuff.split(' ')
        select = [word for word in words if len(word) < 7 or not any(char.isdigit() for char in word)]
        return " ".join(select)
    
    def parse(self, data):
        """
        Parse the book datapoint and check if it fits the token range criteria
        """
        # Start with an empty contents string
        contents = ""
        
        # Add description if available
        if 'description' in data and data['description']:
            if isinstance(data['description'], list):
                contents += '\n'.join(data['description']) + '\n'
            else:
                contents += str(data['description']) + '\n'
        
        # Add features if available
        if 'features' in data and data['features']:
            if isinstance(data['features'], list):
                contents += '\n'.join(data['features']) + '\n'
            else:
                contents += str(data['features']) + '\n'
        
        # Add details if available
        self.details = data.get('details', '')
        if self.details:
            contents += self.clean_text(self.details) + '\n'

            
        # Add categories if available
        if 'categories' in data and data['categories']:
            contents += "Categories: " + ", ".join(data['categories']) + '\n'
            
        # Add author information if available
        if 'author' in data and data['author']:
            contents += "Author: " + str(data['author']) + '\n'
        
        # Check if we have enough content
        if len(contents) > MIN_CHARS:
            # Limit the max size
            contents = contents[:CEILING_CHARS]
            
            # Combine title and contents
            text = f"{self.clean_text(self.title)}\n{self.clean_text(contents)}"
            
            # Tokenize and check if it matches our criteria
            tokens = self.tokenizer.encode(text, add_special_tokens=False)
            if len(tokens) > MIN_TOKENS:
                # Truncate if necessary
                tokens = tokens[:MAX_TOKENS]
                text = self.tokenizer.decode(tokens)
                
                self.make_prompt(text)
                self.include = True
    
    def make_prompt(self, text):
        """Create a standardized prompt for training"""
        self.prompt = f"{self.QUESTION}\n\n{text}\n\n"
        self.prompt += f"{self.PREFIX}{str(round(self.price))}.00"
        self.token_count = len(self.tokenizer.encode(self.prompt, add_special_tokens=False))
    
    def test_prompt(self):
        """Return a prompt suitable for testing, with the actual price removed"""
        return self.prompt.split(self.PREFIX)[0] + self.PREFIX
    
    def __repr__(self):
        """String representation of this BookItem"""
        return f"<{self.title} = ${self.price}>"
