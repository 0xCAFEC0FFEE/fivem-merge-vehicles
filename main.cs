using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace MergeCars
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            addToList("Application successfully loaded.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string folderPath = textBox1.Text;
            if (folderPath == "" || !Directory.Exists(folderPath)) {
                MessageBox.Show("Invalid directory path");
                return;
            }

            button1.Visible = false;
            textBox1.Visible = false;
            listBox1.Visible = true;
            this.Size = new Size(528, 225);

            addToList("Directory found: " + folderPath);

            Thread main = new Thread(new ThreadStart(StartMerging));
            main.Start();
        }

        private void StartMerging()
        {
            string path = textBox1.Text;
            Directory.CreateDirectory(path + @"\.MERGED");
            Directory.CreateDirectory(path + @"\.MERGED\stream");

            addToList("Created folder .MERGED (the new resource)");

            string manifest = path + @"\.MERGED\__resource.lua";

            var myFile = File.Create(manifest);
            myFile.Close();

            File.AppendAllText(manifest, "\r\nresource_manifest_version '77731fab-63ca-442c-a67b-abc70f28dfa5'\r\n\r\nfiles {\r\n");

            string[] cars = Directory.GetDirectories(path);
            var count = 0;

            foreach (var file in cars)
            {
                string folder = Path.GetFileName(file);

                if (folder == ".MERGED" || folder == "stream")
                {
                    continue;
                }

                addToList("Discovered new vehicle: " + folder);

                Directory.CreateDirectory(path + @"\.MERGED\" + folder);

                Copy(file, path + @"\.MERGED\" + folder);

                string streamPath = path + @"\.MERGED\stream\[" + folder + "]";
                Directory.CreateDirectory(streamPath);

                try { Copy(file + @"\stream", streamPath); count++; }
                catch { Directory.Delete(streamPath, false); addToList("!!! Couldn't retrieve streaming assets from " + folder + " !!!"); }
            }


            addToList("Finishing up...");

            File.AppendAllText(manifest, "   '**/carcols.meta',\r\n   '**/carvariations.meta',\r\n   '**/handling.meta',\r\n   '**/vehiclelayouts.meta',\r\n   '**/vehicles.meta'\r\n}\r\n\r\n");
            File.AppendAllText(manifest, "data_file 'VEHICLE_LAYOUTS_FILE'	'**/vehiclelayouts.meta'\r\ndata_file 'HANDLING_FILE'			'**/handling.meta'\r\ndata_file 'VEHICLE_METADATA_FILE'	'**/vehicles.meta'\r\ndata_file 'CARCOLS_FILE'			'**/carcols.meta'\r\ndata_file 'VEHICLE_VARIATION_FILE'	'**/carvariations.meta'");

            addToList("Successfully merged " + count + " resources into 1");
        }

        void Copy(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                if (Path.GetFileName(file) != "__resource.lua") {
                    try 
                    { 
                        File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file))); 
                    } 
                    catch
                    {
                        MessageBox.Show("Please delete all the files then try again.");
                        Environment.Exit(0);
                    }
                        
                }
            }
        }

        private void addToList(string text) {
            this.Invoke(new MethodInvoker(delegate()
            {
                listBox1.Items.Add("[+] " + text);
                int nItems = (int)(listBox1.Height / listBox1.ItemHeight);
                listBox1.TopIndex = listBox1.Items.Count - nItems;
            }));
        }
    }
}
