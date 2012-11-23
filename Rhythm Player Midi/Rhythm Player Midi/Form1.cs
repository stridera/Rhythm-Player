/* Rhythmn Midi -> Player converter
 * 
 * Bugs:  No note-off events?
 * 
 */


using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rhythm_Player_Midi
{
    public partial class Form1 : Form
    {
        // Midi File Format Specification taken from http://faydoc.tripod.com/formats/mid.htm
        // Midi GH Specification from http://slowhero.moto-coda.org/tech/gh3_midi_spec.txt
        public enum TYPE { COMMAND, META };
        public struct NOTE {
            public TYPE type; // C = Command (note to play.)  M = Meta
            public long delta; // Time between each note
            public int value; // The note number if it's a COMMAND, else the meta value
            public string data; // Data string
            public int tempo;
            public int velocity; // The note velocity.  I don't think it's used.
            public int channel; // The channel.  Again, I don't think it's used.
        }
            
        public char[] noteStr = { 'G', 'R', 'Y', 'B', 'O'};
        public List<NOTE> tempos;

        struct MIDI_DATA
        {
            public short numTracks;
            public long tempo;
            public String[] trackNames;
            public List<NOTE>[] tracks;
        }

        Dictionary<int, string> notes = new Dictionary<int, string>{};
        Dictionary<string, int> difficulties = new Dictionary<string, int> { }; // String name plus starting note
 
        byte[] MIDI_HEADER = { 0x4D, 0x54, 0x68, 0x64, 0x00, 0x00, 0x00, 0x06, 0x00, 0x01 }; // MThd + 6 byte header size
        byte[] TRACK_HEADER = {0x4D, 0x54, 0x72, 0x6B}; // MTrk
        MIDI_DATA midi_data = new MIDI_DATA();

        string filename;
        int selectedTrack = -1;
        string difficulty = "Easy";

        public Form1()
        {
            InitializeComponent();

            notes.Add(60, "GREEN, easy (C)");
            notes.Add(61, "RED, easy (C#)");
            notes.Add(62, "YELLOW, easy (D)");
            notes.Add(63, "BLUE, easy (D#)");
            notes.Add(64, "ORANGE, easy (E)");
            notes.Add(67, "star power group, easy (G)");
            notes.Add(69, "player 1 section, easy (A)");
            notes.Add(70, "player 2 section, easy (A#)");
            notes.Add(72, "GREEN, medium (C)");
            notes.Add(73, "RED, medium (C#)");
            notes.Add(74, "YELLOW, medium (D)");
            notes.Add(75, "BLUE, medium (D#)");
            notes.Add(76, "ORANGE, medium (E)");
            notes.Add(79, "star power group, medium (G)");
            notes.Add(81, "player 1 section, medium (A)");
            notes.Add(82, "player 2 section, medium (A#)");
            notes.Add(84, "GREEN, hard (C)");
            notes.Add(85, "RED, hard (C#)");
            notes.Add(86, "YELLOW, hard (D)");
            notes.Add(87, "BLUE, hard (D#)");
            notes.Add(88, "ORANGE, hard (E)");
            notes.Add(91, "star power group, hard (G)");
            notes.Add(93, "player 1 section, hard (A)");
            notes.Add(94, "player 2 section, hard (A#)");
            notes.Add(96, "GREEN, expert (C)");
            notes.Add(97, "RED, expert (C#)");
            notes.Add(98, "YELLOW, expert (D)");
            notes.Add(99, "BLUE, expert (D#)");
            notes.Add(100, "ORANGE, expert (E)");
            notes.Add(103, "star power group, expert (G)");
            notes.Add(105, "player 1 section, expert (A)");
            notes.Add(106, "player 2 section, expert (A#)");
            notes.Add(108, "vocal track (C)");

            difficulties.Add("Easy", 60);
            difficulties.Add("Medium", 72);
            difficulties.Add("Hard", 84);
            difficulties.Add("Expert", 96);


            foreach (string diff in difficulties.Keys)
                difficultyBox.Items.Add(diff);
            difficultyBox.SelectedIndex = 0;
            difficultyBox.Items.Add("Raw");

            tempos = new List<NOTE>();
			
        }

        private void openMidiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                filename = openFileDialog1.FileName;
                this.Text = "Rhythmn Reader Midi - " + System.IO.Path.GetFileName(filename);
                //resetData();
                processMidi(filename);
            }
        }

        private void resetData()
        {


        }

        private void processMidi(string filename)
        {
            BinaryReader midi = new BinaryReader(File.OpenRead(filename));

            readHeader(midi);
            midi_data.trackNames = new String[midi_data.numTracks];
            midi_data.tracks = new List<NOTE>[midi_data.numTracks];
            for (int i = 0; i < midi_data.numTracks; i++)
            {
                midi_data.tracks[i] = new List<NOTE>();
                readTrack(i, midi);
            }

            // If we're still here, it read the file ok
            tempoBox.Text = midi_data.tempo.ToString();

            for (int i = 0; i < midi_data.numTracks; i++)
                trackSelect.Items.Add(midi_data.trackNames[i].ToString());
            trackSelect.Enabled = true;           
        }

        /*
         * readHeader
         *  Takes a Binaryreader and reads the header info
         * 
         * The header chunk appears at the beginning of the file, and describes the file in three ways. The header chunk always looks like:
         *      4D 54 68 64 00 00 00 06 ff ff nn nn dd dd
         *          ff ff : File Type.  Should always be 01 for multiple tracks, synchronous
         *          nn nn : is the number of tracks in the midi file.
         *          dd dd :	is the number of delta-time ticks per quarter note.
         */
        private void readHeader(BinaryReader midi)
        {
            // Verify the header matches
            for (int i = 0; i < MIDI_HEADER.Length; i++)
            {
                if (midi.ReadByte() != MIDI_HEADER[i])
                    throw new FormatException("Invalid Midi File - Header incorrect");
            }

            // Read the rest of the header
            midi_data.numTracks = readShort(midi);
            midi_data.tempo = readShort(midi);
        }

        // We need to swap the bits to get the right endian.
        private short readShort(BinaryReader midi)
        {
            byte[] data = midi.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        // We need to swap the bits to get the right endian.
        private int readInt24(BinaryReader midi)
        {
            midi.ReadByte();
            byte[] data = midi.ReadBytes(3);
            return
                ((int)data[0] << 16) |
                ((int)data[1] << 8) |
                ((int)data[2]);
        }

        /*
         * readTrack
         *  Takes a Binaryreader and reads the track data
         * 
         * The header should be as follows:
         *      4D 54 72 6B xx xx xx xx
         *          xx xx xx xx : Track length
         *          
         *  Event Data is as follows:
         *      dd ff 
         *          dd : Delta Time - A delta time is the number of ticks after which the midi event is to be executed. 
         *                            The number of ticks per quarter note was defined previously in the file header chunk.
         *                            TODO: This can be many bytes long, but for now I only read in one byte since I have yet to see
         *                            multiple byte delta times in a GH song.  Will fix this later
         *          ff : Event - A midi event.  Refer to the midi reference.  If it is 0xFF, it's a meta event, otherwise a normal command.
         */
        private void readTrack(int track, BinaryReader midi)
        {
            // Verify the track header matches and get track length
            for (int i = 0; i < TRACK_HEADER.Length; i++)
            {
                if (midi.ReadByte() != TRACK_HEADER[i])
                    throw new FormatException("Invalid Midi File - Track Header incorrect");
            }

            byte[] length = new Byte[4];
            length = midi.ReadBytes(4);
            long fullDelta = 0;

            // Read the midi events            
            do {
                NOTE n = new NOTE();

                n.delta = readVariableLength(midi);
                fullDelta += n.delta;

                byte flag = midi.ReadByte();
                if (flag == 0xFF)
                {
                    /* Meta Event.  All meta events have the format 0xFF xx nn dd
                     *  xx : The meta command
                     *  nn : The length of data associated with the command
                     *  dd x nn : The data
                     */
                    n.type = TYPE.META;

                    n.value = midi.ReadByte();

                    switch (n.value)
                    {
                        case 0x51:
                            n.tempo = readInt24(midi);

                            NOTE t = new NOTE();
                            t.delta = fullDelta;
                            t.tempo = n.tempo;
                            tempos.Add(t);
                            break;
                        default:
                            n.data = midi.ReadString();
                            break;

                    }

                    if (n.value == 0x03)
                        midi_data.trackNames[track] = n.data;

                    if (n.value == 0x2F)
                        return;
                }
                else
                {
                    n.type = TYPE.COMMAND;

                    int cmd = flag >> 4;
                    n.channel = flag & 0x0F;

                    switch (cmd)
                    {
                        case 0x08: // Note OFF.  Followed by note number and velocity
                            n.value = 0;
                            n.data = String.Format("Note off heard for {0}", midi.ReadByte());
                            n.velocity = midi.ReadByte();
                            break;
                        case 0x09: // Note ON.  Followed by note number and velocity
                            n.value = midi.ReadByte();
                            n.velocity = midi.ReadByte();
                            break;
                        case 0x0A: // Key after-touch.  Followed by Note Number and Velocity
                            n.value = 0;
                            n.data = "Unknown CMD: 0A";
                            midi.ReadByte();
                            midi.ReadByte();
                            break;
                        case 0x0B:
                            n.value = 0;
                            n.data = "Unknown CMD: B";
                            midi.ReadByte();
                            midi.ReadByte();
                            break;
                        case 0x0C:
                            n.value = 0;
                            n.data = "Unknown CMD: C";
                            midi.ReadByte();
                            break;
                        case 0x0D:
                            n.value = 0;
                            n.data = "Unknown CMD: D";
                            midi.ReadByte();
                            break;
                        case 0x0E:
                            n.value = 0;
                            n.data = "Unknown CMD: E";
                            midi.ReadByte();
                            midi.ReadByte();
                            break;
                        default:
                            n.value = 0;
                            n.data = String.Format("Unknown command {0:X}", cmd);
                            break;
                    }
                }

                midi_data.tracks[track].Add(n);

            } while (true);

        }

        private long readVariableLength(BinaryReader midi)
        {
            long length;
            byte tmp;

            if (((length = midi.ReadByte()) & 0x80) > 0)
            {
                length &= 0x7F;
                do
                {
                    length = (length << 7) + ((tmp = midi.ReadByte()) & 0x7F);
                } while ((tmp & 0x80) > 0);
            }

            return length;
        }

        private void trackSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedTrack = trackSelect.SelectedIndex;
            updateTextbox();
        }

        private void difficultyBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            difficulty = difficultyBox.SelectedItem.ToString();
            updateTextbox();
        }

        private void updateTextbox()
        {
            if (midi_data.numTracks == 0 || selectedTrack == -1)
            {
                eventBox.Text = "Load a song and select a track.";
            }
            else if (difficulty == "Raw")
            {
                string txt = "";
                foreach (NOTE n in midi_data.tracks[selectedTrack])
                {
                    txt += String.Format("{0}: ({1}) {2}(0x{2:X}) - {3}\r\n", 
                        n.type == TYPE.META ? "Meta" : "Note",
                        n.delta,
                        n.value,
                        ( n.type == TYPE.META && n.value == 0x51 ) ? n.tempo.ToString() : n.data
                        );
                }
                eventBox.Text = txt;
            }
            else
            {
                long delta = 0;
                long fullDelta = 0;
                int diff = difficulties[difficulty];
                string txt = "";
                string line = "";
                bool newNote = false;
                int tempoIdx = 0;

                foreach (NOTE n in midi_data.tracks[selectedTrack])
                {
                    fullDelta += n.delta;
                    
                    if (newNote && n.delta > 0)
                    {   
                        while(tempoIdx < tempos.Count && tempos[tempoIdx+1].delta < fullDelta)
                            tempoIdx++;
                        txt += delta * ((tempos[tempoIdx].tempo / 1000.0) / midi_data.tempo);
                        txt += "\r\n";
                        txt += line;
                        txt += "S\r\n";
                        line = "";
                        delta = 0;
                        newNote = false;
                    }

                    delta += n.delta;

                    if (n.type == TYPE.COMMAND && n.value >= diff && n.value < diff + 5)
                    {
                        line += noteStr[n.value - diff];
                        newNote = true;
                    }
                }
                eventBox.Text = txt;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(eventBox.Text);
        }

        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = System.IO.Path.GetFileNameWithoutExtension(filename) + "_" + 
                                        midi_data.trackNames[selectedTrack] + "_" +
                                        difficulty + ".txt";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog1.FileName, eventBox.Text);
            }
        }
    }
}
