using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KondziuTheBlockchain
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource cts;
        private Stopwatch stopwatch = new Stopwatch();

        public Block MinedBlock { get; private set; }
        public int BlockID { get; }
        public string PreviousHash { get; }

        public Form1(int blockID, string previousHash)
        {
            InitializeComponent();
            SetupForm();
        }

        //SETUP
        private void SetupForm()
        {
            this.Text = "KondziuTheBlockchain - Proof of Work Mechanism";
            button1.Text = "MINE THIS";
            button1.BackColor = Color.Crimson;
            button1.ForeColor = Color.White;
            button1.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            dataGridView1.BackgroundColor = Color.FromArgb(30, 30, 30);
            dataGridView1.GridColor = Color.FromArgb(60, 60, 60);
            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
            dataGridView1.DefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.RoyalBlue;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            textBox3.Text = "1";     // Block ID
            textBox4.Text = "0";     // Nonce
            textBox2.Text = "waiting to mine...";
            textBox6.Text = PreviousHash;
            textBox6.ReadOnly = true;
            textBox6.Font = new Font("Consolas", 9);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                ofd.Title = "Select Transactions CSV File";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    LoadCsvToGrid(ofd.FileName);
                }
            }
        }

        private void LoadCsvToGrid(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length < 2)
                {
                    throw new Exception("CSV must have header + at least one row");
                }

                // Create DataTable with Transaction_ID
                var dt = new DataTable();
                dt.Columns.Add("Transaction_ID", typeof(int));
                dt.Columns.Add("Date", typeof(string));
                dt.Columns.Add("From", typeof(string));
                dt.Columns.Add("To", typeof(string));
                dt.Columns.Add("Amount", typeof(decimal));

                for (int i = 1; i < lines.Length; i++) // skip header
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length < 4)
                    {
                        continue;
                    }

                    decimal amount = 0m;
                    decimal.TryParse(
                        parts[4].Trim(),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out amount);

                    dt.Rows.Add(
                        int.TryParse(parts[0].Trim(), out int id) ? id : i,   // use CSV ID or fallback to row number
                        parts[1].Trim(),   // Date
                        parts[2].Trim(),   // From
                        parts[3].Trim(),   // To
                        amount             // Amount → now correctly parsed!
                    );
                }

                dataGridView1.DataSource = dt;
                this.Text = $"KondziuTheBlockchain – {dt.Rows.Count} transactions loaded – Ready to mine!";
                MessageBox.Show($"Successfully loaded {dt.Rows.Count} transactions!", "Loaded",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading CSV:\n" + ex.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox3.Text = "1";
            textBox2.Text = "waiting to mine...";
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        //THe mining being done asynchronously to avoid blocking the UI
        private async void button1_Click(object sender, EventArgs e)
        {
            // STOP
            if (button1.Text.Contains("STOP"))
            {
                cts?.Cancel();
                return;
            }

            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Load a CSV first!");
                return;
            }

            if (!int.TryParse(textBox5.Text, out int difficulty) || difficulty < 1 || difficulty > 15)
            {
                MessageBox.Show("Difficulty 1–15");
                return;
            }

            string target = new string('0', difficulty);
            cts = new CancellationTokenSource();

            button1.Text = "STOP MINING";
            button1.BackColor = Color.DarkOrange;
            textBox4.Text = "0";
            textBox2.Text = "mining...";
            stopwatch.Restart();

            // Build block data once
            string blockData = "";
            int txId = 1;
            foreach (DataRow r in ((DataTable)dataGridView1.DataSource).Rows)
            {
                blockData += $"{txId++}|{r["Date"]}|{r["From"]}|{r["To"]}|{r["Amount"]}\n";
            }

            // Run the mining loop on background thread
            try
            {
                await Task.Run(() =>
                {
                    uint nonce = 0;
                    while (!cts.IsCancellationRequested)
                    {
                        nonce++;
                        string header = BlockID + PreviousHash + blockData + nonce;
                        string hash = Sha256BitLevel.ComputeHash(Sha256BitLevel.ComputeHash(header));

                        // Update UI safely from background thread
                        this.Invoke((MethodInvoker)delegate
                        {
                            textBox4.Text = nonce.ToString("#,##0");
                            textBox2.Text = hash;
                            double hps = nonce / stopwatch.Elapsed.TotalSeconds;
                            this.Text = $"Mining Block #{BlockID} | {hps:F1} H/s";
                        });

                        if (hash.StartsWith(target))
                        {
                            // Success! Save result and return to UI thread
                            this.Invoke((MethodInvoker)delegate
                            {
                                MinedBlock = new Block(BlockID, PreviousHash, hash, nonce, difficulty,
                                                      (DataTable)dataGridView1.DataSource);
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                            });
                            return;
                        }
                    }
                }, cts.Token);
            }
            catch (OperationCanceledException)
            {
                
            }

            button1.Text = "MINE THIS";
            button1.BackColor = Color.Crimson;
        }
    }
}
