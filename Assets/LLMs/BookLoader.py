# In BookLoader.py

from tqdm import tqdm
from concurrent.futures import ProcessPoolExecutor
from BookItems import BookItem  # Make sure this is implemented and importable

# Constants
MIN_PRICE = 0.5
MAX_PRICE = 999.49
CHUNK_SIZE = 1000

def from_datapoint(dp):
    try:
        price = float(dp["price"])
        if MIN_PRICE <= price <= MAX_PRICE:
            book_item = BookItem(dp, price)
            return book_item if book_item.include else None
    except (ValueError, TypeError):
        return None

def from_chunk(chunk):
    return [book for book in (from_datapoint(dp) for dp in chunk) if book]

def chunk_generator(rawdata, chunk_size=CHUNK_SIZE):
    size = len(rawdata)
    for i in range(0, size, chunk_size):
        yield rawdata.select(range(i, min(i + chunk_size, size)))

def load_books_from_rawdata(rawdata, workers=8):
    results = []
    with ProcessPoolExecutor(max_workers=workers) as pool:
        for batch in tqdm(pool.map(from_chunk, chunk_generator(rawdata)), total=(len(rawdata) // CHUNK_SIZE) + 1):
            results.extend(batch)
    return results