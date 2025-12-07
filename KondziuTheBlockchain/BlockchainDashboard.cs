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
        }

        private void SetupDashboard()
        {
            this.Text = "KondziuTheBlockchain – Chain View";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(20, 20, 30);

            // Make sure you have FlowLayoutPanel named flowChainPanel in Designer
            flowLayoutPanel1.FlowDirection = FlowDirection.LeftToRight;
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.WrapContents = false;

            CreateGenesisBlock();
        }
        private void CreateGenesisBlock()
        {
            var genesis = new Block(1, new string('0', 64), "", 0, 0, new DataTable());
            chain.Add(genesis);
            AddMiniBlock(genesis);
        }

        private void AddMiniBlock(Block block)
        {
            var p = new Panel
            {
                Size = new Size(180, 240),
                BackColor = string.IsNullOrEmpty(block.Hash) ? Color.FromArgb(70, 70, 70) : Color.FromArgb(0, 140, 0),
                Margin = new Padding(15),
                Tag = block
            };

            p.Controls.Add(new Label { Text = $"Block #{block.ID}", ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true });
            p.Controls.Add(new Label { Text = $"Prev:\n{block.PreviousHash.Substring(0, Math.Min(12, block.PreviousHash.Length))}...", ForeColor = Color.Cyan, Location = new Point(10, 50), AutoSize = true });
            p.Controls.Add(new Label { Text = $"Hash:\n{(string.IsNullOrEmpty(block.Hash) ? "not mined" : block.Hash.Substring(0, 12))}...", ForeColor = Color.Lime, Location = new Point(10, 100), AutoSize = true });
            p.Controls.Add(new Label { Text = $"Nonce: {block.Nonce}", ForeColor = Color.Yellow, Location = new Point(10, 160), AutoSize = true });
            p.Controls.Add(new Label { Text = $"Tx: {block.Transactions.Rows.Count}", ForeColor = Color.Orange, Location = new Point(10, 190), AutoSize = true });

            p.Click += MiniBlock_Click;
            foreach (Control c in p.Controls) c.Click += MiniBlock_Click;

            flowLayoutPanel1.Controls.Add(p);
            flowLayoutPanel1.ScrollControlIntoView(p);
        }

        private void MiniBlock_Click(object sender, EventArgs e)
        {
            var panel = sender is Panel p ? p : (Panel)((Control)sender).Parent;
            var block = (Block)panel.Tag;

            if (!string.IsNullOrEmpty(block.Hash))
            {
                MessageBox.Show("This block is already mined!", "Info");
                return;
            }

            using (var miner = new Form1(block.ID, block.PreviousHash))
            {
                if (miner.ShowDialog() == DialogResult.OK && miner.MinedBlock != null)
                {
                    // Update the block in chain
                    int index = chain.FindIndex(b => b.ID == block.ID);
                    chain[index] = miner.MinedBlock;

                    // Refresh visual
                    flowLayoutPanel1.Controls.Clear();
                    foreach (var b in chain) AddMiniBlock(b);

                    // Add new empty block
                    var next = new Block(block.ID + 1, miner.MinedBlock.Hash, "", 0, 0, new DataTable());
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
