using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rhythm_Player_Client
{
    public partial class Form1 : Form
    {
        // Lowering this to 25 was bad for song 41
        const int STRUM_WAIT = 30;

        const bool DRUM_MODE = true;

        DateTime startTime;
        int currentInstruction = 0;
        int currentNote = 0;
        double wait = 0;
        double lastStrum = 0;
        double lastBassStrum = 0;
        bool isStrumming = false;
        bool isBassStrumming = false;
        bool firstTime = false;
        bool numberRow = true;
        byte output;

        //DO WE MAKE THESE 2 BYTES?
        byte G = 1 << 0;
        byte R = 1 << 1;
        byte Y = 1 << 2;
        byte B = 1 << 3;
        byte O = 1 << 4;
        byte S = 1 << 5;
        byte P = 1 << 6; //I don't know if we'll ever trigger this via input file
        /*byte g = 1 << 7;
        byte r = 1 << 8;
        byte y = 1 << 9;
        byte b = 1 << 10;
        byte o = 1 << 11;
        byte s = 1 << 12;*/

        double tooLate = 0;
        //string[] instructionListCopy;
        //int instructionBuildIndex = 0;

        public Form1()
        {
            InitializeComponent();

            serialPort1.PortName = "COM6";
            serialPort1.BaudRate = 9600;
            serialPort1.DtrEnable = true;
            serialPort1.Open();

            writeByte(0);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = File.ReadAllText(openFileDialog1.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog1.FileName, textBox1.Text);
            }
        }

        //THIS NEEDS TO TAKE 2 BYTES, RIGHT?
        private void writeByte(byte output)
        {
            if (serialPort1.IsOpen)
            {
                byte[] byteArray;
                byteArray = new byte[1];
                byteArray[0] = output;
                serialPort1.Write(byteArray, 0, 1);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //DateTime now = DateTime.Now;

            //TimeSpan elapsed = now - startTime;
            TimeSpan elapsed = DateTime.Now - startTime;
            double timeLeft = wait - elapsed.TotalMilliseconds;

            if (firstTime && elapsed.TotalMilliseconds > 30)
            {
                writeByte(0);
                firstTime = false;
            }

            if (isStrumming && elapsed.TotalMilliseconds - lastStrum > STRUM_WAIT)
            {
                if (DRUM_MODE)
                {
                    writeByte(0);
                }
                else
                {
                    output &= (byte)(output & ~S);
                    writeByte(output);
                }
                isStrumming = false;
            }

            //BASS VERSION, PROBABLY NOT CORRECT
            /*if (isBassStrumming && elapsed.TotalMilliseconds - lastBassStrum > STRUM_WAIT)
            {
                output &= (byte)(output & ~s);
                writeByte(output);
                isBassStrumming = false;
            }*/

            timeLeft = wait - elapsed.TotalMilliseconds;
            if (timeLeft <= 0)
            {
                if (currentInstruction >= listBox1.Items.Count) //instructionBuildIndex)
                {
                    stopRun();
                    return;
                }

                string line = listBox1.Items[currentInstruction].ToString(); //instructionListCopy[currentInstruction];
                

                //double newTempo;
                //if (double.TryParse(line, out newTempo))
                if (numberRow)
                {
                    //double nt = newTempo * Convert.ToDouble(tempoDelayBox.Text);
                    /*double nt = Convert.ToDouble(line);
                    curTempoBox.Text = nt.ToString();
                    wait += nt;*/
                    wait += Convert.ToDouble(line);
                    numberRow = false;

                    //currentInstruction++;
                    //line = listBox1.Items[currentInstruction].ToString();
                }
                else
                {
                    //instrBox.Text = currentInstruction.ToString();
                    //listBox1.SelectedIndex = currentInstruction;
                    //listBox1.TopIndex = currentInstruction - 12;

                    output = 0;
                    if (line.IndexOf('G') != -1)
                    {
                        if (DRUM_MODE)
                        {
                            output = (byte)(output | O);
                        }
                        else
                        {
                            output = (byte)(output | G);
                        }
                    }
                    if (line.IndexOf('R') != -1)
                        output = (byte)(output | R);
                    if (line.IndexOf('Y') != -1)
                        output = (byte)(output | Y);
                    if (line.IndexOf('B') != -1)
                        output = (byte)(output | B);
                    if (line.IndexOf('O') != -1)
                        if (DRUM_MODE)
                        {
                            output = (byte)(output | G);
                        }
                        else
                        {
                            output = (byte)(output | O);
                        }
                    if (line.IndexOf('P') != -1)
                        output = (byte)(output | P);
                    if (line.IndexOf('S') != -1)
                    {
                        lastStrum = wait;
                        output = (byte)(output | S);
                        isStrumming = true;
                    }


                    /*if (line.IndexOf('g') != -1)
                        output = (byte)(output | g);
                    if (line.IndexOf('r') != -1)
                        output = (byte)(output | r);
                    if (line.IndexOf('y') != -1)
                        output = (byte)(output | y);
                    if (line.IndexOf('b') != -1)
                        output = (byte)(output | b);
                    if (line.IndexOf('o') != -1)
                        output = (byte)(output | o);
                    if (line.IndexOf('s') != -1)
                    {
                        lastBassStrum = wait;
                        output = (byte)(output | s);
                        isBassStrumming = true;
                    }*/
                    writeByte(output);


                    //curKeys.Text = Convert.ToString(output, 2);

                    currentNote++;
                    numberRow = true;

                    if (timeLeft <= -15)
                    {
                        if (timeLeft < tooLate)
                        {
                            tooLate = timeLeft;
                        }
                        tempoBox.Text = timeLeft.ToString();
                    }

                }
                currentInstruction++;
                               
                //tempoBox.Text = timeLeft.ToString();
            }
            //tempoBox.Text = ((int) timeLeft).ToString();
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            foreach (string line in textBox1.Lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                Match match = Regex.Match(line.Trim(), @"^(?<keys>[RGBYOSPX]*)\s*(?<tempo>[0-9.]*)$", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string colors = match.Groups["keys"].Value;
                    string number = match.Groups["tempo"].Value;

                    if (colors != "" && number != "")
                    {
                        for (int i = 0; i < int.Parse(number); i++)
                        {
                            listBox1.Items.Add(colors);
                            //instructionListCopy[instructionBuildIndex++] = colors;
                        }
                    }
                    else
                    {
                        if (colors != "")
                        {
                            listBox1.Items.Add(colors);
                            //instructionListCopy[instructionBuildIndex++] = colors;
                        }
                        else
                        {
                            listBox1.Items.Add(number);
                            //instructionListCopy[instructionBuildIndex++] = number;
                        }
                    }
                }
                else
                {
                    //errBox.Text += line + "\r\n";
                }
            }

            textBox1.Visible = false;
            listBox1.Visible = true;

            runToolStripMenuItem.Visible = false;
            stopToolStripMenuItem.Visible = true;

            /*instructionBuildIndex = 0;
            instructionListCopy = new String[textBox1.Lines.Count()];
            foreach (string s in textBox1.Lines)
            {
                instructionListCopy[instructionBuildIndex++] = s;
            }*/
            
            listBox1.SelectedIndex = currentInstruction;
            firstTime = true;
            wait = double.Parse(initWaitBox.Text);
            currentNote = 0;
            startTime = DateTime.Now;
            writeByte(G);
            //ERIC: IS THIS SAFE TO DO IMMEDIATELY?
            //NO!!  //writeByte(0);
            timer1.Interval = 1;
            timer1.Start();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopRun();
        }

        private void stopRun()
        {
            timer1.Stop();
            listBox1.Items.Clear();
            currentInstruction = 0;

            writeByte(0);

            textBox1.Visible = true;
            listBox1.Visible = false;

            runToolStripMenuItem.Visible = true;
            stopToolStripMenuItem.Visible = false;

            numberRow = true;
            tempoBox.Text = tooLate.ToString();
            tooLate = 0.0;
        }

        private void strumButton_Click(object sender, EventArgs e)
        {
            writeByte(S);
            System.Threading.Thread.Sleep(25);
            writeByte(0);
        }

        private void greenButton_Click(object sender, EventArgs e)
        {
            writeByte(G);
            System.Threading.Thread.Sleep(25);
            writeByte(0);
        }

        private void redButton_Click(object sender, EventArgs e)
        {
            writeByte(R);
            System.Threading.Thread.Sleep(25);
            writeByte(0);
        }

        private void yellowButton_Click(object sender, EventArgs e)
        {
            writeByte(Y);
            System.Threading.Thread.Sleep(25);
            writeByte(0);
        }

        private void blueButton_Click(object sender, EventArgs e)
        {
            writeByte(B);
            System.Threading.Thread.Sleep(25);
            writeByte(0);
        }

        private void orangeButton_Click(object sender, EventArgs e)
        {
            writeByte(O);
            System.Threading.Thread.Sleep(25);
            writeByte(0);
        }

        private void tempoDelayBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
    }
}
