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

        const bool DRUM_MODE = false;

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
        byte[] output;

        //DO WE MAKE THESE 2 BYTES?
        byte G = 1 << 0;
        byte R = 1 << 1;
        byte Y = 1 << 2;
        byte B = 1 << 3;
        byte O = 1 << 4;
        byte S = 1 << 5;
        byte P = 1 << 6; //I don't know if we'll ever trigger this via input file

        byte META    = 1 << 7;
        byte START   = 1 << 0;
        byte PLAYERS = 1 << 1;

        double tooLate = 0;
        //string[] instructionListCopy;
        //int instructionBuildIndex = 0;

        public Form1()  
        {
            InitializeComponent();

            serialPort1.PortName = "COM7";
            serialPort1.BaudRate = 9600;
            serialPort1.DtrEnable = true;
            serialPort1.Open();

            writeByte((byte) (META | PLAYERS), 2);
            //writeByte(0, 0);

            output = new byte[2];

            readSerial();
        }

        private void readSerial()
        {
            //serialPort1.ReadExisting();
            /*
            errBox.Text += serialPort1.ReadExisting();
            errBox.SelectionStart = errBox.Text.Length;
            errBox.ScrollToCaret();*/
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
        private void writeByte(byte P1, byte P2)
        {
            if (serialPort1.IsOpen)
            {
                byte[] byteArray;
                byteArray = new byte[1];
                byteArray[0] = P1;
                serialPort1.Write(byteArray, 0, 1);
                byteArray[0] = P2;
                serialPort1.Write(byteArray, 0, 1);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
          // Read from the serial port and output any messages sent
            readSerial();
            //DateTime now = DateTime.Now;

            //TimeSpan elapsed = now - startTime;
            TimeSpan elapsed = DateTime.Now - startTime;
            double timeLeft = wait - elapsed.TotalMilliseconds;

            if (firstTime && elapsed.TotalMilliseconds > 30)
            {
                writeByte(0, 0);
                firstTime = false;
            }

            if (isStrumming && elapsed.TotalMilliseconds - lastStrum > STRUM_WAIT)
            {
                if (DRUM_MODE)
                {
                    writeByte(0, 0);
                }
                else
                {
                    output[0] &= (byte)(output[0] & ~S);
                    output[1] &= (byte)(output[1] & ~S);
                    output[0] &= (byte)(output[0] & ~P);
                    output[1] &= (byte)(output[1] & ~P);
                    writeByte(output[0], output[1]);
                }
                isStrumming = false;
            }


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

                    byte[] tmpOutput = new byte[2];
                    tmpOutput[0] = 0;
                    tmpOutput[1] = 0;
                    int player = 0;
                    foreach(string keys in line.ToUpper().Split('|')) {
                      if (keys.IndexOf('G') != -1)
                      {
                          if (DRUM_MODE)
                          {
                              tmpOutput[player] = (byte)(tmpOutput[player] | O);
                          }
                          else
                          {
                              tmpOutput[player] = (byte)(tmpOutput[player] | G);
                          }
                      }
                      if (keys.IndexOf('R') != -1)
                          tmpOutput[player] = (byte)(tmpOutput[player] | R);
                      if (keys.IndexOf('Y') != -1)
                          tmpOutput[player] = (byte)(tmpOutput[player] | Y);
                      if (keys.IndexOf('B') != -1)
                          tmpOutput[player] = (byte)(tmpOutput[player] | B);
                      if (keys.IndexOf('O') != -1)
                          if (DRUM_MODE)
                          {
                              tmpOutput[player] = (byte)(tmpOutput[player] | G);
                          }
                          else
                          {
                              tmpOutput[player] = (byte)(tmpOutput[player] | O);
                          }
                      //if (keys.IndexOf('P') != -1)
                      //    tmpOutput[player] = (byte)(tmpOutput[player] | P);
                          
                      if (keys.IndexOf('S') != -1)
                      {
                          lastStrum = wait;
                          tmpOutput[player] = (byte)(tmpOutput[player] | P);
                          tmpOutput[player] = (byte)(tmpOutput[player] | S);
                          isStrumming = true;
                      }

                      player++;
                    }
                    if (tmpOutput[0] > 0)
                      output[0] = tmpOutput[0];
                    if (tmpOutput[1] > 0)
                      output[1] = tmpOutput[1];

                    writeByte(output[0], output[1]);


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

                Match match = Regex.Match(line.Trim(), @"^(?<keys>[RGBYOSPX|]*)\s*(?<tempo>[0-9.]*)$", RegexOptions.IgnoreCase);
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

              if (listBox1.Items.Count == 0)
            {
              stopRun();
              return;
            }

            listBox1.SelectedIndex = currentInstruction;
            firstTime = true;
            wait = double.Parse(initWaitBox.Text);
            currentNote = 0;
            startTime = DateTime.Now;
            writeByte(G, 0);
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

            writeByte(0,0);

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
            writeByte(S, 0);
            System.Threading.Thread.Sleep(25);
            writeByte(0, 0);
            readSerial();
        }

        private void greenButton_Click(object sender, EventArgs e)
        {
            writeByte(G, 0);
            System.Threading.Thread.Sleep(25);
            writeByte(0, 0);
            readSerial();
        }

        private void redButton_Click(object sender, EventArgs e)
        {
            writeByte(R, 0);
            System.Threading.Thread.Sleep(25);
            writeByte(0, 0);
            readSerial();
        }

        private void yellowButton_Click(object sender, EventArgs e)
        {
            writeByte(Y, 0);
            System.Threading.Thread.Sleep(25);
            writeByte(0, 0);
            readSerial();
        }

        private void blueButton_Click(object sender, EventArgs e)
        {
            writeByte(B, 0);
            System.Threading.Thread.Sleep(25);
            writeByte(0, 0);
            readSerial();
        }

        private void orangeButton_Click(object sender, EventArgs e)
        {
            writeByte(O,0);
            System.Threading.Thread.Sleep(25);
            writeByte(0, 0);
            readSerial();
        }

        private void p2strum_Click(object sender, EventArgs e)
        {
          writeByte(0, S);
          System.Threading.Thread.Sleep(25);
          writeByte(0, 0);
          readSerial();
        }

        private void p2green_Click(object sender, EventArgs e)
        {
          writeByte(0, G);
          System.Threading.Thread.Sleep(25);
          writeByte(0, 0);
          readSerial();
        }

        private void p2red_Click(object sender, EventArgs e)
        {
          writeByte(0, R);
          System.Threading.Thread.Sleep(25);
          writeByte(0, 0);
          readSerial();
        }

        private void p2yellow_Click(object sender, EventArgs e)
        {
          writeByte(0, Y);
          System.Threading.Thread.Sleep(25);
          writeByte(0, 0);
          readSerial();
        }

        private void p2blue_Click(object sender, EventArgs e)
        {
          writeByte(0, B);
          System.Threading.Thread.Sleep(25);
          writeByte(0, 0);
          readSerial();
        }

        private void p2orange_Click(object sender, EventArgs e)
        {
          writeByte(0, O);
          System.Threading.Thread.Sleep(25);
          writeByte(0, 0);
          readSerial();
        }

        private void startbtn_Click(object sender, EventArgs e)
        {
          writeByte((byte)(META | START), 0);
          System.Threading.Thread.Sleep(25);
          writeByte(0, 0);  
          readSerial();
        }

        private void button1_Click(object sender, EventArgs e)
        {
          writeByte((byte)(META | PLAYERS), 2);
          System.Threading.Thread.Sleep(25);
          writeByte(0, 0);
          readSerial();
        }

        private void backbtn_Click(object sender, EventArgs e)
        {
          writeByte(P, 0);
          System.Threading.Thread.Sleep(25);
          writeByte(0, 0);
          readSerial();
        }

    }
}
