const express = require("express");
const { MongoClient } = require("mongodb");
const dotenv = require("dotenv");
dotenv.config();

const app = express();
const port = 3000;

const mongoUri = process.env.MONGO_URI;
const client = new MongoClient(mongoUri, {
  useNewUrlParser: true,
  useUnifiedTopology: true,
});

app.use(express.json());

async function connectToMongo() {
  try {
    // Kết nối tới MongoDB
    await client.connect();
    console.log("MongoDB connection successful!");
  } catch (error) {
    console.error("MongoDB connection failed:", error);
  }
}

app.get("/api/players", async (req, res) => {
  try {
    const db = client.db("game_db");
    const players = db.collection("players");
    const allPlayers = await players.find().toArray();
    res.json(allPlayers);
  } catch (error) {
    res.status(500).send("Error fetching players");
  }
});

// Đảm bảo kết nối MongoDB trước khi API hoạt động
connectToMongo().then(() => {
  app.listen(port, () => {
    console.log(`API running on http://localhost:${port}`);
  });
});
