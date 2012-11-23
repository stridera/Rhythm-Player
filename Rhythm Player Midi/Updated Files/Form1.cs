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
        public bool RB_MODE = true;

        // Midi File Format Specification taken from http://faydoc.tripod.com/formats/mid.htm
        // Midi GH Specification from http://slowhero.moto-coda.org/tech/gh3_midi_spec.txt
        // RB1 MIDI Info for each instrument links from here: http://creators.rockband.com/docs/Reaper
        public enum TYPE { COMMAND, META };
        public struct NOTE {
            public TYPE type; // C = Command (note to play.)  M = Meta
            public string eventName; // human-readable name for the event
            public long delta; // Time between each note
            public long absoluteTicks; // Ticks from start of song before executing this Note
            public int value; // The note number if it's a COMMAND, else the meta value
            public string data; // Data string
            public int tempo;
            public int velocity; // The note velocity.  I don't think it's used. May mean how quickly the note is pressed for analog instruments
            public int channel; // The channel.  Again, I don't think it's used. 
        }

        public char BASS_STRUM = 's';
        public char GUITAR_STRUM = 'S';
        public char[] noteStr       = { 'G', 'R', 'Y', 'B', 'O'};
        public char[] bassNoteStr   = { 'g', 'r', 'y', 'b', 'o' };
        public List<NOTE> tempos;

        struct MIDI_DATA
        {
            public short fileFormat;
            public short numTracks;
            public long tempo;
            public String[] trackNames;
            public List<NOTE>[] tracks;
        }

        Dictionary<int, string> notes = new Dictionary<int, string>{};
        Dictionary<string, int> difficulties = new Dictionary<string, int> { }; // String name plus starting note

        //!!!TEMPORARY GET THE FILE FORMAT
        byte[] MIDI_HEADER = { 0x4D, 0x54, 0x68, 0x64, 0x00, 0x00, 0x00, 0x06 };//, 0x00, 0x01 }; // MThd + 6 byte header size
        byte[] TRACK_HEADER = {0x4D, 0x54, 0x72, 0x6B}; // MTrk
        MIDI_DATA midi_data = new MIDI_DATA();

        string filename;
        int selectedTrack = -1;
        string difficulty = "Easy";

        int guitarTrackIndex = 0;
        int bassTrackIndex = 0;

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
            notes.Add(103, "star power group, expert (G)"); //aka solo
            notes.Add(105, "player 1 section, expert (A)");
            notes.Add(106, "player 2 section, expert (A#)");
            notes.Add(108, "vocal track (C)");
            notes.Add(110, "yellow Pro TOM");
            notes.Add(111, "blue Pro TOM");
            notes.Add(112, "green Pro TOM");
            notes.Add(120, "drum fill green");
            notes.Add(121, "drum fill red");
            notes.Add(122, "drum fill yellow");
            notes.Add(123, "drum fill blue");
            notes.Add(124, "drum fill orange");
            notes.Add(126, "Standard Drum Roll");
            notes.Add(127, "Special Drum Roll");

            //What's 116? Track 1 has it...

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
            midi_data.trackNames = new String[midi_data.numTracks + 1];
            midi_data.tracks = new List<NOTE>[midi_data.numTracks];

            for (int i = 0; i < midi_data.numTracks; i++)
            {
                midi_data.tracks[i] = new List<NOTE>();
                readTrack(i, midi);
            }

            // If we're still here, it read the file ok
            tempoBox.Text = midi_data.tempo.ToString();

            for (int i = 0; i < midi_data.numTracks; i++) {
                string iTrackName = midi_data.trackNames[i];
                if (iTrackName == null)
                {
                    iTrackName = "null";
                }
                trackSelect.Items.Add(iTrackName.ToString());
                //trackSelect.Items.Add(midi_data.trackNames[i].ToString());
            }

            midi_data.trackNames[midi_data.numTracks] = "CO-OP";
            trackSelect.Items.Add("CO-OP");

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
            midi_data.fileFormat = readShort(midi);
            midi_data.numTracks = readShort(midi);
            midi_data.tempo = readShort(midi); //delta-time ticks per quarter note
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
            try
            {
                // Verify the track header matches and get track length
                for (int i = 0; i < TRACK_HEADER.Length; i++)
                {
                    if (midi.ReadByte() != TRACK_HEADER[i])
                        throw new FormatException("Invalid Midi File - Track Header incorrect" + TRACK_HEADER[i].ToString());
                }
            }
            catch (EndOfStreamException e)
            {
                //out of bytes!
                Console.WriteLine("Error writing the data.");
                Console.WriteLine(e.ToString());
            }
            byte[] length = new Byte[4];
            //THIS LENGTH IS NOT VERIFIED
            length = midi.ReadBytes(4); 
            long fullDelta = 0;

            // Read the midi events            
            do {
                NOTE n = new NOTE();

                //delta-time: execute the midi-event after this number of ticks
                n.delta = readVariableLength(midi);
                //total number of ticks?
                fullDelta += n.delta;
                n.absoluteTicks = fullDelta;

                //META or COMMAND. Assuming that there's no omissision of the command itself, which would have meant "execute the last command again"
                byte flag;
                try
                {
                    flag = midi.ReadByte();
                }
                catch (EndOfStreamException e)
                {
                    //out of bytes!
                    Console.WriteLine("Error writing the data.");
                    Console.WriteLine(e.ToString());
                    return;
                }
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
                        //SET TEMPO EVENT
                        case 0x51:
                            n.tempo = readInt24(midi);
                            n.eventName = "TEMPO    ";
                            NOTE t = new NOTE();
                            t.delta = fullDelta;
                            t.absoluteTicks = fullDelta;
                            t.tempo = n.tempo;
                            tempos.Add(t);
                            break;
                        
                        //0x58 could be interesting:time signature
                        case 0x58:
                            n.data = midi.ReadString();
                            char[] timesigData = n.data.ToCharArray();
                            int numerator = timesigData[0];
                            int denominator = timesigData[1];
                            int ticksPerClick = timesigData[2];
                            int thirtySecondToQuarter = timesigData[3];
                            n.data = String.Format("{0}/{1}, {2}, {3}",
                                numerator,
                                denominator,
                                ticksPerClick,
                                thirtySecondToQuarter
                            );
                            n.eventName = "TIMESIG  ";
                            //n.data = midi.ReadString().Length.ToString();
                            break;
                       
                        default:
                            //does this work for all cases? Does it get all the bytes? Are they readable?
                            n.data = midi.ReadString();
                            break;

                    }

                    //SEQUENCE OR TRACK NAME
                    //This could probably be a Dictionary.
                    if (n.value == 0x00)
                    {
                        n.eventName = "TRACKSEQ ";
                    }
                    else if (n.value == 0x01)
                    {
                        n.eventName = "TEXTANY  ";
                    }
                    else if (n.value == 0x02)
                    {
                        n.eventName = "TEXTCOPY ";
                    }
                    else if (n.value == 0x03) {
                        midi_data.trackNames[track] = n.data;
                        if (n.data == "PART GUITAR")
                        {
                            guitarTrackIndex = track;
                        }
                        else 
                        {
                            if (RB_MODE && n.data == "PART DRUMS")
                            {
                                bassTrackIndex = track;
                            }
                            else if (!RB_MODE && n.data == "PART BASS")
                            {
                                bassTrackIndex = track;
                            }
                        }
                        n.eventName = "TRACKNAME";
                    }
                    else if (n.value == 0x04)
                    {
                        n.eventName = "INSTNAME ";
                    }
                    else if (n.value == 0x05)
                    {
                        n.eventName = "LYRIC    ";
                    }
                    else if (n.value == 0x06)
                    {
                        n.eventName = "MARKER   ";
                    }
                    else if (n.value == 0x07)
                    {
                        n.eventName = "CUEPOINT ";
                    }
                    else if (n.value == 0x59)
                    {
                        n.eventName = "KEYSIG   ";
                    }
                    else if (n.value == 0x7F)
                    {
                        n.eventName = "SEQUENCER";
                    }
                    //END OF TRACK
                    if (n.value == 0x2F)
                        return;
                     
                }
                else
                {
                    n.type = TYPE.COMMAND;

                    //top 4 bits of first byte is the command
                    int cmd = flag >> 4;
                    //last 4 bits is the channel number
                    n.channel = flag & 0x0F;

                    switch (cmd)
                    {
                        case 0x08: // Note OFF.  Followed by note number and velocity
                            n.value = 0;
                            n.data = String.Format("Note off heard for note number {0}", midi.ReadByte());
                            n.velocity = midi.ReadByte();
                            break;
                        case 0x09: // Note ON.  Followed by note number and velocity
                            // WHY NO DATA SET HERE?
                            n.value = midi.ReadByte();
                            n.data = "Note ON";
                            n.velocity = midi.ReadByte();
                            break;
                        case 0x0A: // Key after-touch.  Followed by Note Number and Velocity
                            n.value = 0;
                            n.data = "BAD CMD: Key after-touch";
                            n.value = midi.ReadByte();
                            n.velocity = midi.ReadByte();
                            break;
                        case 0x0B: // Control Change. Followed by controller number and new value
                            n.value = 0;
                            n.data = "BAD CMD: Control Change";
                            n.value = midi.ReadByte();
                            n.velocity = midi.ReadByte();
                            break;
                        case 0x0C: // Program (patch) change). Followed by new program name
                            n.value = 0;
                            n.data = "BAD CMD: Program change";
                            n.value = midi.ReadByte();
                            break;
                        case 0x0D: // Channel after-touch. Followed by channel number
                            n.value = 0;
                            n.data = "BAD CMD: Channel after-touch";
                            n.value = midi.ReadByte();
                            break;
                        case 0x0E: // Pitch wheel change followed by 2 bytes of value
                            n.value = 0;
                            n.data = "BAD CMD: Pitch wheel";
                            n.value = midi.ReadByte();
                            n.velocity = midi.ReadByte();
                            break;
                        default:
                            if (RB_MODE)
                            {
                                n.value = flag;//midi.ReadByte();
                                n.data = "Note ON";
                                n.velocity = midi.ReadByte();
                            }
                            else
                            {
                                n.value = 0;
                                n.data = String.Format("Unknown command {0:X}", cmd);
                            }
                            break;

                    }
                }
                //if (!RB_MODE || n.velocity == 100)
                //{
                midi_data.tracks[track].Add(n);
                //}

            } while (true);

        }

        private long readVariableLength(BinaryReader midi)
        {
            long length = 0; //does this variable name indicate something?
            byte tmp;

            try
            {
                if (((length = midi.ReadByte()) & 0x80) > 0)
                {
                    length &= 0x7F;
                    do //this should be a max of 4 bytes
                    {
                        //first bit of 0 indicates that was the last byte
                        //remaining 7 bits are useful data
                        length = (length << 7) + ((tmp = midi.ReadByte()) & 0x7F);
                    } while ((tmp & 0x80) > 0);
                    //checking that first bit is 1, because 0 means that was the last byte of the value
                }
            }
            catch (EndOfStreamException e)
            {
                //out of bytes!
                Console.WriteLine("Error writing the data.");
                Console.WriteLine(e.ToString());
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
                string txt = String.Format("Format {0}\tTracks {1}\tTempo {2}\tTempos {3}\r\n\r\n",
                    midi_data.fileFormat,
                    midi_data.numTracks,
                    midi_data.tempo,
                    tempos.Count
                    );
                txt += "NAME:      \tTICKS\tABSOL\tVALUE (HEX) \tDATA w/Velocity\r\n\r\n";
                foreach (NOTE n in midi_data.tracks[selectedTrack])
                {
                    string value = n.value.ToString();
                    if (n.type == TYPE.COMMAND)
                    {
                        if (notes.ContainsKey(n.value))
                        {
                            value = notes[n.value];
                        }
                    }
                    //NAME:   TICK_DELAY   ABSOLUTE_DELAY   VALUE (hex) - DATA
                    txt += String.Format("{0}:\t{1}\t{2}\t{3} (0x{3:X})      \t{4}\r\n",
                        n.type == TYPE.META ? n.eventName : "Note         ",
                        n.delta,
                        n.absoluteTicks,
                        value,
                        (n.type == TYPE.META && n.value == 0x51) ? n.tempo.ToString() : ((n.type == TYPE.COMMAND) ? n.data.ToString() + " " + n.velocity : n.data)
                    );
                }
                eventBox.Text = txt;
            }
            else if (selectedTrack == midi_data.numTracks) //CO-OP
            {
                int diff = difficulties[difficulty];
                string txt = ""; //final string to display in the text box

                string line = "";
                string bassLine = "";
                bool guitarStrum = false; //if there are guitar notes, strum guitar
                bool bassStrum = false; //if there are bass notes, strum bass

                int guitarIndex = 0;
                int bassIndex = 0;
                int tempoIndex = 0; // array index of tempo change events
                long now = 0;       // absolute ticks since the start of song that have already been calculated
                long currentTempo = midi_data.tempo;  // tempo value of the tempo change event currently being evaluated
                double nDelayInMS = 0; // calculated sum of milliseconds to wait since the last note before playing the current note

                List<NOTE> guitarNotes = midi_data.tracks[guitarTrackIndex];
                List<NOTE> bassNotes = midi_data.tracks[bassTrackIndex];

                int guitarSize = guitarNotes.Count;
                int bassSize = bassNotes.Count;
                int tempoSize = tempos.Count;

                while (guitarIndex < guitarSize || bassIndex < bassSize)
                {
                    while (guitarIndex < guitarSize &&
                        guitarNotes[guitarIndex].absoluteTicks == now)
                    {
                        if (isNotePlayed(guitarNotes[guitarIndex], diff))
                        {
                            line += noteStr[guitarNotes[guitarIndex].value - diff];
                            guitarStrum = true;
                        }
                        guitarIndex++;
                    }

                    while (bassIndex < bassSize &&
                        bassNotes[bassIndex].absoluteTicks == now)
                    {
                        if (isNotePlayed(bassNotes[bassIndex], diff))
                        {
                            bassLine += bassNoteStr[bassNotes[bassIndex].value - diff];
                            bassStrum = true;
                        }
                        bassIndex++;
                    }

                    if (line.Length > 0 || bassLine.Length > 0)
                    {
                        txt += nDelayInMS + "\r\n";

                        if(guitarStrum) line += GUITAR_STRUM;
                        if(bassStrum) bassLine += BASS_STRUM;

                        if (line.Length > 0) txt += line;
                        if (bassLine.Length > 0) txt += "|" + bassLine;

                        txt += "\r\n";

                        nDelayInMS = 0;
                        line = "";
                        bassLine = "";
                        guitarStrum = false;
                        bassStrum = false;
                    }

                    //corner case
                    while (tempoIndex < tempos.Count &&
                        tempos[tempoIndex].absoluteTicks == now)
                    {
                        currentTempo = tempos[tempoIndex].tempo;
                        tempoIndex++;
                    }

                    //advance note tracks to a playable note.
                    while (guitarIndex < guitarSize && !isNotePlayed(guitarNotes[guitarIndex], diff))
                    {
                        guitarIndex++;
                    }
                    while (bassIndex < bassSize && !isNotePlayed(bassNotes[bassIndex], diff))
                    {
                        bassIndex++;
                    }

                    //still have tempos and have at least one note track left
                    //Calculate all tempo change delays before the next playable note in either track
                    while ((tempoIndex < tempos.Count) && 
                        (guitarIndex >= guitarSize || tempos[tempoIndex].absoluteTicks < guitarNotes[guitarIndex].absoluteTicks) &&
                        (bassIndex >= bassSize || tempos[tempoIndex].absoluteTicks < bassNotes[bassIndex].absoluteTicks))
                    {
                        // Add them together
                        nDelayInMS += calculateMSDelay(tempos[tempoIndex].absoluteTicks, now, currentTempo, midi_data.tempo);
                        currentTempo = tempos[tempoIndex].tempo;
                        now = tempos[tempoIndex].absoluteTicks;
                        tempoIndex++;
                    }

                    if (guitarIndex < guitarSize && (bassIndex >= bassSize ||
                        (guitarNotes[guitarIndex].absoluteTicks <= bassNotes[bassIndex].absoluteTicks)))
                    {
                        nDelayInMS += calculateMSDelay(guitarNotes[guitarIndex].absoluteTicks, now, currentTempo, midi_data.tempo);
                        now = guitarNotes[guitarIndex].absoluteTicks;
                    }
                    else if (bassIndex < bassSize)
                    {
                        nDelayInMS += calculateMSDelay(bassNotes[bassIndex].absoluteTicks, now, currentTempo, midi_data.tempo);
                        now = bassNotes[bassIndex].absoluteTicks;
                    }
                }

                if (nDelayInMS > 0)
                {
                    while (tempoIndex < tempos.Count) {
                        nDelayInMS += calculateMSDelay(tempos[tempoIndex].absoluteTicks, now, currentTempo, midi_data.tempo);
                        currentTempo = tempos[tempoIndex].tempo;
                        now = tempos[tempoIndex].absoluteTicks;
                        tempoIndex++;
                    }
                    if (guitarIndex < guitarSize && guitarNotes[guitarIndex - 1].absoluteTicks > now)
                    {
                        nDelayInMS += calculateMSDelay(guitarNotes[guitarIndex].absoluteTicks, now, currentTempo, midi_data.tempo);
                        now = guitarNotes[guitarIndex - 1].absoluteTicks;
                    }
                    if (bassIndex < bassSize && bassNotes[bassIndex - 1].absoluteTicks > now)
                    {
                        nDelayInMS += calculateMSDelay(bassNotes[bassIndex].absoluteTicks, now, currentTempo, midi_data.tempo);
                    }
                    txt += nDelayInMS + "\r\n" + line + "G\r\n"; ;
                }
                eventBox.Text = txt;
            }
            else //if (RB_MODE)
            {
                int diff = difficulties[difficulty];
                string txt = ""; //final string to display in the text box

                string line = "";
                bool doStrum = false; //if there are guitar notes, strum guitar
                
                int noteIndex = 0;
                int tempoIndex = 0; // array index of tempo change events
                long now = 0;       // absolute ticks since the start of song that have already been calculated
                long currentTempo = midi_data.tempo;  // tempo value of the tempo change event currently being evaluated
                double nDelayInMS = 0; // calculated sum of milliseconds to wait since the last note before playing the current note

                List<NOTE> trackNotes = midi_data.tracks[selectedTrack];
                
                int noteSize = trackNotes.Count;
                int tempoSize = tempos.Count;

                while (noteIndex < noteSize)
                {
                    while (noteIndex < noteSize &&
                        trackNotes[noteIndex].absoluteTicks == now)
                    {
                        if (isNotePlayed(trackNotes[noteIndex], diff))
                        {
                            line += noteStr[trackNotes[noteIndex].value - diff];
                            doStrum = true;
                        }
                        noteIndex++;
                    }

                    if (line.Length > 0)
                    {
                        txt += nDelayInMS + "\r\n";

                        if (doStrum) line += GUITAR_STRUM;
                        if (line.Length > 0) txt += line;

                        txt += "\r\n";

                        nDelayInMS = 0;
                        line = "";
                        doStrum = false;
                    }

                    //corner case
                    while (tempoIndex < tempos.Count &&
                        tempos[tempoIndex].absoluteTicks == now)
                    {
                        currentTempo = tempos[tempoIndex].tempo;
                        tempoIndex++;
                    }

                    //advance note tracks to a playable note.
                    while (noteIndex < noteSize && !isNotePlayed(trackNotes[noteIndex], diff))
                    {
                        noteIndex++;
                    }

                    //still have tempos and have at least one note track left
                    //Calculate all tempo change delays before the next playable note in either track
                    while ((tempoIndex < tempos.Count) &&
                        (noteIndex >= noteSize || tempos[tempoIndex].absoluteTicks < trackNotes[noteIndex].absoluteTicks))
                    {
                        // Add them together
                        nDelayInMS += calculateMSDelay(tempos[tempoIndex].absoluteTicks, now, currentTempo, midi_data.tempo);
                        currentTempo = tempos[tempoIndex].tempo;
                        now = tempos[tempoIndex].absoluteTicks;
                        tempoIndex++;
                    }

                    if (noteIndex < noteSize)
                    {
                        nDelayInMS += calculateMSDelay(trackNotes[noteIndex].absoluteTicks, now, currentTempo, midi_data.tempo);
                        now = trackNotes[noteIndex].absoluteTicks;
                    }
                }

                if (nDelayInMS > 0 || tempoIndex < tempos.Count || noteIndex < noteSize)
                {
                    while (tempoIndex < tempos.Count)
                    {
                        nDelayInMS += calculateMSDelay(tempos[tempoIndex].absoluteTicks, now, currentTempo, midi_data.tempo);
                        currentTempo = tempos[tempoIndex].tempo;
                        now = tempos[tempoIndex].absoluteTicks;
                        tempoIndex++;
                    }
                    if (noteIndex < noteSize && trackNotes[noteIndex - 1].absoluteTicks > now)
                    {
                        nDelayInMS += calculateMSDelay(trackNotes[noteIndex].absoluteTicks, now, currentTempo, midi_data.tempo);
                        now = trackNotes[noteIndex - 1].absoluteTicks;
                    }
                    txt += nDelayInMS + "\r\n" + line + "G\r\n"; ;
                }
                eventBox.Text = txt;
            }
            //THIS WORKED FOR GH, BUT ABOVE VERSION CATCHES MORE CORNER CASES
            /*else
            {
                int diff = difficulties[difficulty];
                string txt = "";
                string line = "";

                // ERIC additions
                //!! BUG: LAST NOTE IS NOT SUSTAINED!
                int tempoIndex = 0; // array index of tempo change events
                long now = 0;       // absolute ticks since the start of song that have already been calculated
                long currentTempo = midi_data.tempo;  // tempo value of the tempo change event currently being evaluated
                double nDelayInMS = 0; // calculated sum of milliseconds to wait since the last note before playing the current note

                List<NOTE> trackNotes = midi_data.tracks[selectedTrack];
                int trackSize = trackNotes.Count;
                for (int noteIndex = 0; noteIndex < trackSize; noteIndex++)
                {
                    NOTE n = trackNotes[noteIndex];

                    // META events in instrument tracks aren't displayed and aren't read by the RPC 
                    if (n.type == TYPE.COMMAND)
                    {
                        // ERIC additions
                        if (n.delta > 0)
                        {
                            // Calculate the MS for each of the tempo changes since the last note and before the current note.
                            // Most notes will not have a tempo change between them.
                            // Tempo changes exactly at this note will only affect later notes.
                            while (tempoIndex < tempos.Count &&
                                    tempos[tempoIndex].absoluteTicks < n.absoluteTicks)
                            {
                                // Add them together
                                nDelayInMS += calculateMSDelay(tempos[tempoIndex].absoluteTicks, now, currentTempo, midi_data.tempo);
                                currentTempo = tempos[tempoIndex].tempo;
                                now = tempos[tempoIndex].absoluteTicks;
                                tempoIndex++;
                            }

                            // Calculate the MS between the last tempo change and the current note.
                            // Add it to the total
                            nDelayInMS += calculateMSDelay(n.absoluteTicks, now, currentTempo, midi_data.tempo);
                            now = n.absoluteTicks;
                        }

                        // this is currently the only way to detect a NOTE OFF command
                        // Track 1 has a case that needs them to affect the tempo counts above:
                        //Note         :	180	9840	0 (0x0)      	Note off heard for note number 60 127
                        //Note         :	0	9840	GREEN, expert (C) (0xGREEN, expert (C))      	Note ON 127
                        if (n.value == 0)
                        {
                            continue;
                        }
                        if (RB_MODE && n.velocity == 0)
                        {
                            continue;
                        }
                        // Note values are within a span of 6 for each difficulty
                        if (isNotePlayedInDifficulty(n.value, diff))
                        {
                            line += noteStr[n.value - diff];
                        }

                        //Sometimes, there's no note for given difficulty within a block of NOTE ON commands.
                        //So only write the line when it's non-empty; not just when there's a delta in the next note.
                        if (noteIndex + 1 >= trackSize ||
                            (trackNotes[noteIndex + 1].delta > 0 && line.Length > 0))
                        {
                            txt += nDelayInMS + "\r\n" + line + "S\r\n";
                            line = "";
                            nDelayInMS = 0;
                        }
                    }

                }
                //HACK TO HOLD LAST NOTE
                txt += "700000\r\nGS\r\n";
                eventBox.Text = txt;
            }*/
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(eventBox.Text);
        }

        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //!!! CAN'T SAVE CO-OP BECAUSE IT ISN'T PROPERLY IN THE ARRAY
            saveFileDialog1.FileName = System.IO.Path.GetFileNameWithoutExtension(filename) + "_" + 
                                        midi_data.trackNames[selectedTrack] + "_" +
                                        difficulty + ".txt";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog1.FileName, eventBox.Text);
            }
        }

        // Magical function to return the number of milliseconds between now
        // and absoluteTicks given the tempo information
        private double calculateMSDelay(long absoluteTicks, long now,
                long currentTempo, long headerTempo)
        {
            return (absoluteTicks - now) * ((currentTempo / 1000.0) / headerTempo);
        }

        // Returns positive if the note's integer value is within the range of
        // the difficulty's lower bound
        // Note values are within a span of 6 for each difficulty
        private bool isNotePlayedInDifficulty(int noteValue, int difficultyStart)
        {
            return noteValue >= difficultyStart && noteValue < (difficultyStart + 5);
        }

        private bool isNotePlayed(NOTE n, int difficultyStart)
        {
            return (n.type == TYPE.COMMAND) && (n.value > 0) &&
                isNotePlayedInDifficulty(n.value, difficultyStart) && (!RB_MODE || n.velocity > 0);
                //(n.value >= difficultyStart) && (n.value < (difficultyStart + 5));
        }
    }
}
