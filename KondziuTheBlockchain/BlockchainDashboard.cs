using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KondziuTheBlockchain
{
    public partial class BlockchainDashboard : Form
    {
        private List<Block> chain = new List<Block>();
        public BlockchainDashboard()
        {
            InitializeComponent();
            SetupDashboard();
        }

        private void SetupDashboard()
        {
            this.Text = "KondziuTheBlockchain – Chain View";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(20, 20, 30);
            this.Size = new Size(1200, 600);

            // Make sure you have FlowLayoutPanel named flowChainPanel in Designer
            flowLayoutPanel1.FlowDirection = FlowDirection.LeftToRight;
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.WrapContents = false;

            CreateGenesisBlock();
        }
        private void CreateGenesisBlock()
        {
            var genesis = new Block(
                id: 0,
                previousHash: new string('0', 64),
                hash: new string('0', 64),     // fake mined 0th hash
                nonce: 0,
                difficulty: 0,
                transactions: new DataTable()
            );

            chain.Add(genesis);
            AddMiniBlock(genesis);

            // BLOCK #1 — first real block to mine
            var firstBlock = new Block(
                id: 1,
                previousHash: genesis.Hash,    // points to Block #0
                hash: "",                      // not mined yet
                nonce: 0,
                difficulty: 0,
                transactions: new DataTable()
            );
            chain.Add(firstBlock);
            AddMiniBlock(firstBlock);
        }
        
        //The block design
        private void AddMiniBlock(Block block)
        {
            bool isMined = !string.IsNullOrEmpty(block.Hash) && block.Hash != "";

            var p = new Panel
            {
                Size = new Size(200, 300),
                BackColor = isMined ? Color.FromArgb(0, 180, 0) : Color.FromArgb(70, 70, 70),
                Margin = new Padding(15),
                BorderStyle = BorderStyle.FixedSingle,
                Tag = block
            };

            p.Controls.Add(new Label
            {
                Text = $"Block #{block.ID}",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(10, 15),
                AutoSize = true
            });

            p.Controls.Add(new Label
            {
                Text = $"Prev:\n{block.PreviousHash.Substring(0, 10)}...",
                ForeColor = Color.Cyan,
                Location = new Point(10, 60),
                AutoSize = true
            });

            p.Controls.Add(new Label
            {
                Text = $"Hash:\n{(isMined ? block.Hash.Substring(0, 10) + "..." : "not mined")}",
                ForeColor = isMined ? Color.Lime : Color.OrangeRed,
                Location = new Point(10, 110),
                AutoSize = true
            });

            p.Controls.Add(new Label
            {
                Text = $"Nonce: {block.Nonce}",
                ForeColor = Color.Yellow,
                Location = new Point(10, 180),
                AutoSize = true
            });

            p.Controls.Add(new Label
            {
                Text = $"Tx: {block.Transactions.Rows.Count}",
                ForeColor = Color.Orange,
                Location = new Point(10, 210),
                AutoSize = true
            });

            // Only unmixed blocks are clickable
            if (!isMined)
            {
                p.Click += MiniBlock_Click;
                foreach (Control c in p.Controls) c.Click += MiniBlock_Click;
                p.Cursor = Cursors.Hand;
            }

            flowLayoutPanel1.Controls.Add(p);
            flowLayoutPanel1.ScrollControlIntoView(p);
        }

        private void MiniBlock_Click(object sender, EventArgs e)
        {
            var panel = sender is Panel p ? p : (Panel)((Control)sender).Parent;
            var block = (Block)panel.Tag;

            if (!string.IsNullOrEmpty(block.Hash))
            {
                MessageBox.Show("Already mined!");
                return;
            }

            using (var miner = new Form1(block.ID, block.PreviousHash))
            {
                if (miner.ShowDialog() == DialogResult.OK && miner.MinedBlock != null)
                {
                    block.Hash = miner.MinedBlock.Hash;
                    block.Nonce = miner.MinedBlock.Nonce;
                    block.Difficulty = miner.MinedBlock.Difficulty;
                    block.Transactions = miner.MinedBlock.Transactions;

                    // Refresh visuals
                    flowLayoutPanel1.Controls.Clear();
                    foreach (var b in chain)
                    {
                        AddMiniBlock(b);
                    }

                    // Add next block
                    var next = new Block(block.ID + 1, block.Hash, "", 0, 0, new DataTable());
                    chain.Add(next);
                    AddMiniBlock(next);
                }
            }
        }

        private void BlockchainDashboard_Load(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
