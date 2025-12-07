using System;
using System.Data;

namespace KondziuTheBlockchain
{
    public class Block
    {
        public int ID { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public uint Nonce { get; set; }
        public int Difficulty { get; set; }
        public DataTable Transactions { get; set; }

        //Constructor - to create the blocks easily
        public Block(int id, string previousHash, string hash, uint nonce, int difficulty, DataTable transactions)
        {
            ID = id;
            PreviousHash = previousHash ?? new string('0', 64);
            Hash = hash ?? "";
            Nonce = nonce;
            Difficulty = difficulty;
            Transactions = transactions?.Copy() ?? new DataTable(); // safe copy
        }

        //Block 0
        public Block() : this(1, new string('0', 64), "", 0, 0, new DataTable()) { }
    }
}