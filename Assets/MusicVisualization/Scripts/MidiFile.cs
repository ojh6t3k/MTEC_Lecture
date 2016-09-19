using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;


[Serializable]
public class MidiEvent
{
    public int DeltaTime;        /** The time between the previous event and this on */
    public int StartTime;        /** The absolute time this event occurs */
    public bool HasEventflag;    /** False if this is using the previous eventflag */
    public byte EventFlag;       /** NoteOn, NoteOff, etc.  Full list is in class MidiFile */
    public byte Channel;         /** The channel this event occurs on */

    public byte Notenumber;      /** The note number  */
    public byte Velocity;        /** The volume of the note */
    public byte Instrument;      /** The instrument */
    public byte KeyPressure;     /** The key pressure */
    public byte ChanPressure;    /** The channel pressure */
    public byte ControlNum;      /** The controller number */
    public byte ControlValue;    /** The controller value */
    public ushort PitchBend;     /** The pitch bend value */
    public byte Numerator;       /** The numerator, for TimeSignature meta events */
    public byte Denominator;     /** The denominator, for TimeSignature meta events */
    public int Tempo;            /** The tempo, for Tempo meta events */
    public byte Metaevent;       /** The metaevent, used if eventflag is MetaEvent */
    public int Metalength;       /** The metaevent length  */
    public byte[] Value;         /** The raw byte value, for Sysex and meta events */

    public MidiEvent()
    {
    }

    /** Return a copy of this event */
    public MidiEvent Clone()
    {
        MidiEvent mevent = new MidiEvent();
        mevent.DeltaTime = DeltaTime;
        mevent.StartTime = StartTime;
        mevent.HasEventflag = HasEventflag;
        mevent.EventFlag = EventFlag;
        mevent.Channel = Channel;
        mevent.Notenumber = Notenumber;
        mevent.Velocity = Velocity;
        mevent.Instrument = Instrument;
        mevent.KeyPressure = KeyPressure;
        mevent.ChanPressure = ChanPressure;
        mevent.ControlNum = ControlNum;
        mevent.ControlValue = ControlValue;
        mevent.PitchBend = PitchBend;
        mevent.Numerator = Numerator;
        mevent.Denominator = Denominator;
        mevent.Tempo = Tempo;
        mevent.Metaevent = Metaevent;
        mevent.Metalength = Metalength;
        mevent.Value = Value;
        return mevent;
    }
}

/** @class MidiNote
* A MidiNote contains
*
* starttime - The time (measured in pulses) when the note is pressed.
* channel   - The channel the note is from.  This is used when matching
*             NoteOff events with the corresponding NoteOn event.
*             The channels for the NoteOn and NoteOff events must be
*             the same.
* notenumber - The note number, from 0 to 127.  Middle C is 60.
* duration  - The time duration (measured in pulses) after which the 
*             note is released.
*
* A MidiNote is created when we encounter a NoteOff event.  The duration
* is initially unknown (set to 0).  When the corresponding NoteOff event
* is found, the duration is set by the method NoteOff().
*/
[Serializable]
public class MidiNote : IComparer<MidiNote>
{
    [SerializeField]
    private int starttime;   /** The start time, in pulses */
    [SerializeField]
    private int channel;     /** The channel */
    [SerializeField]
    private int notenumber;  /** The note, from 0 to 127. Middle C is 60 */
    [SerializeField]
    private int duration;    /** The duration, in pulses */


    /* Create a new MidiNote.  This is called when a NoteOn event is
        * encountered in the MidiFile.
        */
    public MidiNote(int starttime, int channel, int notenumber, int duration)
    {
        this.starttime = starttime;
        this.channel = channel;
        this.notenumber = notenumber;
        this.duration = duration;
    }

    public int StartTime
    {
        get { return starttime; }
        set { starttime = value; }
    }

    public int EndTime
    {
        get { return starttime + duration; }
    }

    public int Channel
    {
        get { return channel; }
    }

    public int Number
    {
        get { return notenumber; }
        set { notenumber = value; }
    }

    public int Duration
    {
        get { return duration; }
        set { duration = value; }
    }

    /* A NoteOff event occurs for this note at the given time.
    * Calculate the note duration based on the noteoff event.
    */
    public void NoteOff(int endtime)
    {
        duration = endtime - starttime;
    }

    /** Compare two MidiNotes based on their start times.
        *  If the start times are equal, compare by their numbers.
        */
    public int Compare(MidiNote x, MidiNote y)
    {
        if (x.StartTime == y.StartTime)
            return x.Number - y.Number;
        else
            return x.StartTime - y.StartTime;
    }

    public MidiNote Clone()
    {
        return new MidiNote(starttime, channel, notenumber, duration);
    }

    public override
    string ToString()
    {
        string[] scale = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
        return string.Format("MidiNote channel={0} number={1} {2} start={3} duration={4}",
                                channel, notenumber, scale[(notenumber + 3) % 12], starttime, duration);

    }

}
/** @class MidiTrack
    * The MidiTrack takes as input the raw MidiEvents for the track, and gets:
    * - The list of midi notes in the track.
    * - The first instrument used in the track.
    *
    * For each NoteOn event in the midi file, a new MidiNote is created
    * and added to the track, using the AddNote() method.
    * 
    * The NoteOff() method is called when a NoteOff event is encountered,
    * in order to update the duration of the MidiNote.
    */

[Serializable]
public class MidiTrack
{
    [SerializeField]
    private int tracknum;            /** The track number */
    [SerializeField]
    private List<MidiNote> notes;    /** List of Midi notes */
    [SerializeField]
    private int instrument;          /** Instrument for this track */

    /** Create an empty MidiTrack.  Used by the Clone method */
    public MidiTrack(int tracknum)
    {
        this.tracknum = tracknum;
        notes = new List<MidiNote>(20);
        instrument = 0;
    }

    /** Create a MidiTrack based on the Midi events.  Extract the NoteOn/NoteOff
    *  events to gather the list of MidiNotes.
    */
    public MidiTrack(List<MidiEvent> events, int tracknum)
    {
        this.tracknum = tracknum;
        notes = new List<MidiNote>(events.Count);
        instrument = 0;

        foreach (MidiEvent mevent in events)
        {
            if (mevent.EventFlag == MidiFile.EventNoteOn && mevent.Velocity > 0)
            {
                MidiNote note = new MidiNote(mevent.StartTime, mevent.Channel, mevent.Notenumber, 0);
                AddNote(note);
            }
            else if (mevent.EventFlag == MidiFile.EventNoteOn && mevent.Velocity == 0)
            {
                NoteOff(mevent.Channel, mevent.Notenumber, mevent.StartTime);
            }
            else if (mevent.EventFlag == MidiFile.EventNoteOff)
            {
                NoteOff(mevent.Channel, mevent.Notenumber, mevent.StartTime);
            }
            else if (mevent.EventFlag == MidiFile.EventProgramChange)
            {
                instrument = mevent.Instrument;
            }
        }
        if (notes.Count > 0 && notes[0].Channel == 9)
        {
            instrument = 128;  /* Percussion */
        }
    }

    public int Number
    {
        get { return tracknum; }
    }

    public List<MidiNote> Notes
    {
        get { return notes; }
    }

    public int Instrument
    {
        get { return instrument; }
        set { instrument = value; }
    }

    public string InstrumentName
    {
        get
        {
            if (instrument >= 0 && instrument <= 128)
                return MidiFile.Instruments[instrument];
            else
                return "";
        }
    }

    /** Add a MidiNote to this track.  This is called for each NoteOn event */
    public void AddNote(MidiNote m)
    {
        notes.Add(m);
    }

    /** A NoteOff event occured.  Find the MidiNote of the corresponding
     * NoteOn event, and update the duration of the MidiNote.
     */
    public void NoteOff(int channel, int notenumber, int endtime)
    {
        for (int i = notes.Count - 1; i >= 0; i--)
        {
            MidiNote note = notes[i];
            if (note.Channel == channel && note.Number == notenumber &&
                note.Duration == 0)
            {
                note.NoteOff(endtime);
                return;
            }
        }
    }

    /** Return a deep copy clone of this MidiTrack. */
    public MidiTrack Clone()
    {
        MidiTrack track = new MidiTrack(Number);
        track.instrument = instrument;
        foreach (MidiNote note in notes)
        {
            track.notes.Add(note.Clone());
        }
        return track;
    }
    public override string ToString()
    {
        string result = "Track number=" + tracknum + " instrument=" + instrument + "\n";
        foreach (MidiNote n in notes)
        {
            result = result + n + "\n";
        }
        result += "End Track\n";
        return result;
    }
}

/** @class MidiFileException
 * A MidiFileException is thrown when an error occurs
 * while parsing the Midi File.  The constructore takes
 * the file offset (in bytes) where the error occurred,
 * and a string describing the error.
 */
public class MidiFileException : System.Exception
{
    public MidiFileException(string s, int offset) :
        base(s + " at offset " + offset)
    {
    }
}

/** @class MidiFileReader
 * The MidiFileReader is used to read low-level binary data from a file.
 * This class can do the following:
 *
 * - Peek at the next byte in the file.
 * - Read a byte
 * - Read a 16-bit big endian short
 * - Read a 32-bit big endian int
 * - Read a fixed length ascii string (not null terminated)
 * - Read a "variable length" integer.  The format of the variable length
 *   int is described at the top of this file.
 * - Skip ahead a given number of bytes
 * - Return the current offset.
 */

public class MidiFileReader
{
    private byte[] data;       /** The entire midi file data */
    private int parse_offset;  /** The current offset while parsing */

    /** Create a new MidiFileReader for the given filename */
    public MidiFileReader(string filename)
    {
        FileStream file = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        FileInfo info = new FileInfo(filename);

        if (!info.Exists)
        {
            throw new MidiFileException("File " + filename + " does not exist", 0);
        }
        if (info.Length == 0)
        {
            throw new MidiFileException("File " + filename + " is empty (0 bytes)", 0);
        }


        /* Read the entire file into memory */
        data = new byte[info.Length];
        int offset = 0;
  //      int len = (int)info.Length;
        while (true)
        {
            if (offset == info.Length)
                break;
            int n = file.Read(data, offset, (int)(info.Length - offset));
            if (n <= 0)
                break;
            offset += n;
        }
        file.Close();

        parse_offset = 0;
    }

    /** Create a new MidiFileReader from the given data */
    public MidiFileReader(byte[] bytes)
    {
        data = bytes;
        parse_offset = 0;
    }

    /** Check that the given number of bytes doesn't exceed the file size */
    private void checkRead(int amount)
    {
        if (parse_offset + amount > data.Length)
        {
            throw new MidiFileException("File is truncated", parse_offset);
        }
    }

    /** Read the next byte in the file, but don't increment the parse offset */
    public byte Peek()
    {
        checkRead(1);
        return data[parse_offset];
    }

    /** Read a byte from the file */
    public byte ReadByte()
    {
        checkRead(1);
        byte x = data[parse_offset];
        parse_offset++;
        return x;
    }

    /** Read the given number of bytes from the file */
    public byte[] ReadBytes(int amount)
    {
        checkRead(amount);
        byte[] result = new byte[amount];
        for (int i = 0; i < amount; i++)
        {
            result[i] = data[i + parse_offset];
        }
        parse_offset += amount;
        return result;
    }

    /** Read a 16-bit short from the file */
    public ushort ReadShort()
    {
        checkRead(2);
        ushort x = (ushort)((data[parse_offset] << 8) | data[parse_offset + 1]);
        parse_offset += 2;
        return x;
    }

    /** Read a 32-bit int from the file */
    public int ReadInt()
    {
        checkRead(4);
        int x = (int)((data[parse_offset] << 24) | (data[parse_offset + 1] << 16) |
                       (data[parse_offset + 2] << 8) | data[parse_offset + 3]);
        parse_offset += 4;
        return x;
    }

    /** Read an ascii string with the given length */
    public string ReadAscii(int len)
    {
        checkRead(len);
        string s = ASCIIEncoding.ASCII.GetString(data, parse_offset, len);
        parse_offset += len;
        return s;
    }

    /** Read a variable-length integer (1 to 4 bytes). The integer ends
     * when you encounter a byte that doesn't have the 8th bit set
     * (a byte less than 0x80).
     */
    public int ReadVarlen()
    {
        uint result = 0;
        byte b;

        b = ReadByte();
        result = (uint)(b & 0x7f);

        for (int i = 0; i < 3; i++)
        {
            if ((b & 0x80) != 0)
            {
                b = ReadByte();
                result = (uint)((result << 7) + (b & 0x7f));
            }
            else
            {
                break;
            }
        }
        return (int)result;
    }

    /** Skip over the given number of bytes */
    public void Skip(int amount)
    {
        checkRead(amount);
        parse_offset += amount;
    }

    /** Return the current parse offset */
    public int GetOffset()
    {
        return parse_offset;
    }

    /** Return the raw midi file byte data */
    public byte[] GetData()
    {
        return data;
    }
}

/** @class MidiSoundOptions
 *
 * The MidiSoundOptions class contains the available options for
 * modifying the midi sound/sound during playback. 
 */
public class MidiSoundOptions
{
    public int tempo;        /** The tempo, in microseconds per quarter note */
    public int transpose;    /** The amount to transpose each note by */
    public int shifttime;    /** Shift the start time by the given amount */
    public int pauseTime;    /** Start the midi music at the given pause time */
    public bool useDefaultInstruments; /** If true, don't change instruments */
    public int[] instruments;  /** The instruments to use per track */
    //public bool[] tracks;       /** Which tracks to include (true = include) */
}

/** @class SheetMusicOptions
* The SheetMusicOptions class contains the available options for
* modifying the midi sheet music.
*/
public class SheetMusicOptions
{
    public bool[] tracks;         /** Which tracks to display (true = display) */
    public bool scrollVert;       /** Whether to scroll vertically or horizontally */
    public bool largeNoteSize;    /** Display large or small note sizes */
    public bool twoStaffs;        /** Combine tracks into two staffs ? */
    public int shifttime;         /** Shift note starttimes by the given amount */
    public int transpose;         /** Shift note key up/down by given amount */
    public object key;            /** Use the given KeySignature */
    public TimeSignature time;    /** Use the given time signature */
    public int combineInterval;   /** Combine notes within given time interval (msec) */

    public SheetMusicOptions()
    {
        tracks = null;
        scrollVert = false;
        largeNoteSize = false;
        twoStaffs = false;
        shifttime = 0;
        transpose = 0;
        key = null;
        time = null;
    }
}

/** @class MidiFile
*
* The MidiFile class contains the parsed data from the Midi File.
* It contains:
* - All the tracks in the midi file, including all MidiNotes per track.
* - The time signature (e.g. 4/4, 3/4, 6/8)
* - The number of pulses per quarter note.
* - The tempo (number of microseconds per quarter note).
*
* The constructor takes a filename as input, and upon returning,
* contains the parsed data from the midi file.
*
* The methods ReadTrack() and ReadMetaEvent() are helper functions called
* by the constructor during the parsing.
*
* After the MidiFile is parsed and created, the user can retrieve the 
* tracks and notes by using the property Tracks and Tracks.Notes.
*
* There are two methods for modifying the midi data based on the menu
* options selected:
*
* - ChangeSheetMusicOptions()
*   Apply the menu options to the parsed MidiFile.  This uses the helper functions:
*     SplitTrack()
*     CombineToTwoTracks()
*     ShiftTime()
*     Transpose()
*     RoundStartTimes()
*     RoundDurations()
*
* - ChangeSound()
*   Apply the menu options to the MIDI music data, and save the modified midi data 
*   to a file, for playback.  This uses the helper functions:
*     AddTempoEvent()
*     ChangeSoundPerChannel
*   
*/

[Serializable]
public class MidiFile
{
    public string filename;          /** The Midi file name */
    [SerializeField]
    private List<MidiEvent>[] events; /** The raw MidiEvents, one list per track */
    [SerializeField]
    private List<MidiTrack> tracks;  /** The tracks of the midifile that have notes */
    [SerializeField]
    private ushort trackmode;         /** 0 (single track), 1 (simultaneous tracks) 2 (independent tracks) */
    [SerializeField]
    private TimeSignature timesig;    /** The time signature */
    [SerializeField]
    private int quarternote;          /** The number of pulses per quarter note */
    [SerializeField]
    private int totalpulses;          /** The total length of the song, in pulses */
    [SerializeField]
    private bool trackPerChannel;     /** True if we've split each channel into a track */

    /* The list of Midi Events */
    public const int EventNoteOff = 0x80;
    public const int EventNoteOn = 0x90;
    public const int EventKeyPressure = 0xA0;
    public const int EventControlChange = 0xB0;
    public const int EventProgramChange = 0xC0;
    public const int EventChannelPressure = 0xD0;
    public const int EventPitchBend = 0xE0;
    public const int SysexEvent1 = 0xF0;
    public const int SysexEvent2 = 0xF7;
    public const int MetaEvent = 0xFF;

    /* The list of Meta Events */
    public const int MetaEventSequence = 0x0;
    public const int MetaEventText = 0x1;
    public const int MetaEventCopyright = 0x2;
    public const int MetaEventSequenceName = 0x3;
    public const int MetaEventInstrument = 0x4;
    public const int MetaEventLyric = 0x5;
    public const int MetaEventMarker = 0x6;
    public const int MetaEventEndOfTrack = 0x2F;
    public const int MetaEventTempo = 0x51;
    public const int MetaEventSMPTEOffset = 0x54;
    public const int MetaEventTimeSignature = 0x58;
    public const int MetaEventKeySignature = 0x59;

    /* The Program Change event gives the instrument that should
     * be used for a particular channel.  The following table
     * maps each instrument number (0 thru 128) to an instrument
     * name.
     */
    public static string[] Instruments = {

        "Acoustic Grand Piano",
        "Bright Acoustic Piano",
        "Electric Grand Piano",
        "Honky-tonk Piano",
        "Electric Piano 1",
        "Electric Piano 2",
        "Harpsichord",
        "Clavi",
        "Celesta",
        "Glockenspiel",
        "Music Box",
        "Vibraphone",
        "Marimba",
        "Xylophone",
        "Tubular Bells",
        "Dulcimer",
        "Drawbar Organ",
        "Percussive Organ",
        "Rock Organ",
        "Church Organ",
        "Reed Organ",
        "Accordion",
        "Harmonica",
        "Tango Accordion",
        "Acoustic Guitar (nylon)",
        "Acoustic Guitar (steel)",
        "Electric Guitar (jazz)",
        "Electric Guitar (clean)",
        "Electric Guitar (muted)",
        "Overdriven Guitar",
        "Distortion Guitar",
        "Guitar harmonics",
        "Acoustic Bass",
        "Electric Bass (finger)",
        "Electric Bass (pick)",
        "Fretless Bass",
        "Slap Bass 1",
        "Slap Bass 2",
        "Synth Bass 1",
        "Synth Bass 2",
        "Violin",
        "Viola",
        "Cello",
        "Contrabass",
        "Tremolo Strings",
        "Pizzicato Strings",
        "Orchestral Harp",
        "Timpani",
        "String Ensemble 1",
        "String Ensemble 2",
        "SynthStrings 1",
        "SynthStrings 2",
        "Choir Aahs",
        "Voice Oohs",
        "Synth Voice",
        "Orchestra Hit",
        "Trumpet",
        "Trombone",
        "Tuba",
        "Muted Trumpet",
        "French Horn",
        "Brass Section",
        "SynthBrass 1",
        "SynthBrass 2",
        "Soprano Sax",
        "Alto Sax",
        "Tenor Sax",
        "Baritone Sax",
        "Oboe",
        "English Horn",
        "Bassoon",
        "Clarinet",
        "Piccolo",
        "Flute",
        "Recorder",
        "Pan Flute",
        "Blown Bottle",
        "Shakuhachi",
        "Whistle",
        "Ocarina",
        "Lead 1 (square)",
        "Lead 2 (sawtooth)",
        "Lead 3 (calliope)",
        "Lead 4 (chiff)",
        "Lead 5 (charang)",
        "Lead 6 (voice)",
        "Lead 7 (fifths)",
        "Lead 8 (bass + lead)",
        "Pad 1 (new age)",
        "Pad 2 (warm)",
        "Pad 3 (polysynth)",
        "Pad 4 (choir)",
        "Pad 5 (bowed)",
        "Pad 6 (metallic)",
        "Pad 7 (halo)",
        "Pad 8 (sweep)",
        "FX 1 (rain)",
        "FX 2 (soundtrack)",
        "FX 3 (crystal)",
        "FX 4 (atmosphere)",
        "FX 5 (brightness)",
        "FX 6 (goblins)",
        "FX 7 (echoes)",
        "FX 8 (sci-fi)",
        "Sitar",
        "Banjo",
        "Shamisen",
        "Koto",
        "Kalimba",
        "Bag pipe",
        "Fiddle",
        "Shanai",
        "Tinkle Bell",
        "Agogo",
        "Steel Drums",
        "Woodblock",
        "Taiko Drum",
        "Melodic Tom",
        "Synth Drum",
        "Reverse Cymbal",
        "Guitar Fret Noise",
        "Breath Noise",
        "Seashore",
        "Bird Tweet",
        "Telephone Ring",
        "Helicopter",
        "Applause",
        "Gunshot",
        "Percussion"
    };
    /* End Instruments */

    /** Return a string representation of a Midi event */
    private string EventName(int ev)
    {
        if (ev >= EventNoteOff && ev < EventNoteOff + 16)
            return "NoteOff";
        else if (ev >= EventNoteOn && ev < EventNoteOn + 16)
            return "NoteOn";
        else if (ev >= EventKeyPressure && ev < EventKeyPressure + 16)
            return "KeyPressure";
        else if (ev >= EventControlChange && ev < EventControlChange + 16)
            return "ControlChange";
        else if (ev >= EventProgramChange && ev < EventProgramChange + 16)
            return "ProgramChange";
        else if (ev >= EventChannelPressure && ev < EventChannelPressure + 16)
            return "ChannelPressure";
        else if (ev >= EventPitchBend && ev < EventPitchBend + 16)
            return "PitchBend";
        else if (ev == MetaEvent)
            return "MetaEvent";
        else if (ev == SysexEvent1 || ev == SysexEvent2)
            return "SysexEvent";
        else
            return "Unknown";
    }

    /** Return a string representation of a meta-event */
    private string MetaName(int ev)
    {
        if (ev == MetaEventSequence)
            return "MetaEventSequence";
        else if (ev == MetaEventText)
            return "MetaEventText";
        else if (ev == MetaEventCopyright)
            return "MetaEventCopyright";
        else if (ev == MetaEventSequenceName)
            return "MetaEventSequenceName";
        else if (ev == MetaEventInstrument)
            return "MetaEventInstrument";
        else if (ev == MetaEventLyric)
            return "MetaEventLyric";
        else if (ev == MetaEventMarker)
            return "MetaEventMarker";
        else if (ev == MetaEventEndOfTrack)
            return "MetaEventEndOfTrack";
        else if (ev == MetaEventTempo)
            return "MetaEventTempo";
        else if (ev == MetaEventSMPTEOffset)
            return "MetaEventSMPTEOffset";
        else if (ev == MetaEventTimeSignature)
            return "MetaEventTimeSignature";
        else if (ev == MetaEventKeySignature)
            return "MetaEventKeySignature";
        else
            return "Unknown";
    }


    /** Get the list of tracks */
    public List<MidiTrack> Tracks
    {
        get { return tracks; }
    }

    /** Get the time signature */
    public TimeSignature Time
    {
        get { return timesig; }
    }

    /** Get the file name */
    public string FileName
    {
        get { return filename; }
    }

    /** Get the total length (in pulses) of the song */
    public int TotalPulses
    {
        get { return totalpulses; }
    }

    /** Parse the given Midi file, and return an instance of this MidiFile
 * class.  After reading the midi file, this object will contain:
 * - The raw list of midi events
 * - The Time Signature of the song
 * - All the tracks in the song which contain notes. 
 * - The number, starttime, and duration of each note.
 */
	public MidiFile(string filename)
	{
		this.filename = filename;
		Load(new MidiFileReader(filename));

	}

	public MidiFile(byte[] data)
	{
		Load(new MidiFileReader(data));
	}

	public void Load(MidiFileReader file)
    {
        string id;
        int len;
        
        tracks = new List<MidiTrack>();
        trackPerChannel = false;
		        
        id = file.ReadAscii(4);
        if (id != "MThd")
        {
            throw new MidiFileException("Doesn't start with MThd", 0);
        }
        len = file.ReadInt();
        if (len != 6)
        {
            throw new MidiFileException("Bad MThd header", 4);
        }
        trackmode = file.ReadShort();
        int num_tracks = file.ReadShort();
        quarternote = file.ReadShort();

        events = new List<MidiEvent>[num_tracks];
        for (int tracknum = 0; tracknum < num_tracks; tracknum++)
        {
            events[tracknum] = ReadTrack(file);
            MidiTrack track = new MidiTrack(events[tracknum], tracknum);
            if (track.Notes.Count > 0)
            {
                tracks.Add(track);
            }
        }

        /* Get the length of the song in pulses */
        foreach (MidiTrack track in tracks)
        {
            MidiNote last = track.Notes[track.Notes.Count - 1];
            if (this.totalpulses < last.StartTime + last.Duration)
            {
                this.totalpulses = last.StartTime + last.Duration;
            }
        }

        /* If we only have one track with multiple channels, then treat
         * each channel as a separate track.
         */
        if (tracks.Count == 1 && HasMultipleChannels(tracks[0]))
        {
            tracks = SplitChannels(tracks[0], events[tracks[0].Number]);
            trackPerChannel = true;
        }

        CheckStartTimes(tracks);

        /* Determine the time signature */
        int tempo = 0;
        int numer = 0;
        int denom = 0;
        foreach (List<MidiEvent> list in events)
        {
            foreach (MidiEvent mevent in list)
            {
                if (mevent.Metaevent == MetaEventTempo && tempo == 0)
                {
                    //Debug.Log("mevent.Tempo : " + mevent.Tempo);
                    tempo = mevent.Tempo;
                }
                if (mevent.Metaevent == MetaEventTimeSignature && numer == 0)
                {
                    numer = mevent.Numerator;
                    denom = mevent.Denominator;
                }
            }
        }
        if (tempo == 0)
        {
            tempo = 500000; /* 500,000 microseconds = 0.05 sec */
            //tempo = 120;
        }
        if (numer == 0)
        {
            numer = 4; denom = 4;
        }
        timesig = new TimeSignature(numer, denom, quarternote, tempo);
    }
    /** Parse a single Midi track into a list of MidiEvents.
 * Entering this function, the file offset should be at the start of
 * the MTrk header.  Upon exiting, the file offset should be at the
 * start of the next MTrk header.
 */
    private List<MidiEvent> ReadTrack(MidiFileReader file)
    {
        List<MidiEvent> result = new List<MidiEvent>(20);
        int starttime = 0;
        string id = file.ReadAscii(4);

        if (id != "MTrk")
        {
            throw new MidiFileException("Bad MTrk header", file.GetOffset() - 4);
        }
        int tracklen = file.ReadInt();
        int trackend = tracklen + file.GetOffset();

        int eventflag = 0;

        while (file.GetOffset() < trackend)
        {

            // If the midi file is truncated here, we can still recover.
            // Just return what we've parsed so far.

//            int startoffset = 0;
            int deltatime = 0;
            byte peekevent;
            try
            {
    //            startoffset = file.GetOffset();
                deltatime = file.ReadVarlen();
                starttime += deltatime;
                peekevent = file.Peek();
            }
            catch (MidiFileException e)
            {
                e.ToString();
                return result;
            }

            MidiEvent mevent = new MidiEvent();
            result.Add(mevent);
            mevent.DeltaTime = deltatime;
            mevent.StartTime = starttime;

            if (peekevent >= EventNoteOff)
            {
                mevent.HasEventflag = true;
                eventflag = file.ReadByte();
            }

            // Console.WriteLine("offset {0}: event {1} {2} start {3} delta {4}", 
            //                   startoffset, eventflag, EventName(eventflag), 
            //                   starttime, mevent.DeltaTime);

            if (eventflag >= EventNoteOn && eventflag < EventNoteOn + 16)
            {
                mevent.EventFlag = EventNoteOn;
                mevent.Channel = (byte)(eventflag - EventNoteOn);
                mevent.Notenumber = file.ReadByte();
                mevent.Velocity = file.ReadByte();
            }
            else if (eventflag >= EventNoteOff && eventflag < EventNoteOff + 16)
            {
                mevent.EventFlag = EventNoteOff;
                mevent.Channel = (byte)(eventflag - EventNoteOff);
                mevent.Notenumber = file.ReadByte();
                mevent.Velocity = file.ReadByte();
            }
            else if (eventflag >= EventKeyPressure &&
                     eventflag < EventKeyPressure + 16)
            {
                mevent.EventFlag = EventKeyPressure;
                mevent.Channel = (byte)(eventflag - EventKeyPressure);
                mevent.Notenumber = file.ReadByte();
                mevent.KeyPressure = file.ReadByte();
            }
            else if (eventflag >= EventControlChange &&
                     eventflag < EventControlChange + 16)
            {
                mevent.EventFlag = EventControlChange;
                mevent.Channel = (byte)(eventflag - EventControlChange);
                mevent.ControlNum = file.ReadByte();
                mevent.ControlValue = file.ReadByte();
            }
            else if (eventflag >= EventProgramChange &&
                     eventflag < EventProgramChange + 16)
            {
                mevent.EventFlag = EventProgramChange;
                mevent.Channel = (byte)(eventflag - EventProgramChange);
                mevent.Instrument = file.ReadByte();
            }
            else if (eventflag >= EventChannelPressure &&
                     eventflag < EventChannelPressure + 16)
            {
                mevent.EventFlag = EventChannelPressure;
                mevent.Channel = (byte)(eventflag - EventChannelPressure);
                mevent.ChanPressure = file.ReadByte();
            }
            else if (eventflag >= EventPitchBend &&
                     eventflag < EventPitchBend + 16)
            {
                mevent.EventFlag = EventPitchBend;
                mevent.Channel = (byte)(eventflag - EventPitchBend);
                mevent.PitchBend = file.ReadShort();
            }
            else if (eventflag == SysexEvent1)
            {
                mevent.EventFlag = SysexEvent1;
                mevent.Metalength = file.ReadVarlen();
                mevent.Value = file.ReadBytes(mevent.Metalength);
            }
            else if (eventflag == SysexEvent2)
            {
                mevent.EventFlag = SysexEvent2;
                mevent.Metalength = file.ReadVarlen();
                mevent.Value = file.ReadBytes(mevent.Metalength);
            }
            else if (eventflag == MetaEvent)
            {
                mevent.EventFlag = MetaEvent;
                mevent.Metaevent = file.ReadByte();
                mevent.Metalength = file.ReadVarlen();
                mevent.Value = file.ReadBytes(mevent.Metalength);
                if (mevent.Metaevent == MetaEventTimeSignature)
                {
                    if (mevent.Metalength != 4)
                    {
                        throw new MidiFileException(
                          "Meta Event Time Signature len == " + mevent.Metalength +
                          " != 4", file.GetOffset());
                    }
                    mevent.Numerator = (byte)mevent.Value[0];
                    mevent.Denominator = (byte)System.Math.Pow(2, mevent.Value[1]);
                }
                else if (mevent.Metaevent == MetaEventTempo)
                {
                    if (mevent.Metalength != 3)
                    {
                        throw new MidiFileException(
                          "Meta Event Tempo len == " + mevent.Metalength +
                          " != 3", file.GetOffset());
                    }
                    mevent.Tempo = ((mevent.Value[0] << 16) | (mevent.Value[1] << 8) | mevent.Value[2]);
                }
                else if (mevent.Metaevent == MetaEventEndOfTrack)
                {
                    /* break;  */
                }
            }
            else
            {
                throw new MidiFileException("Unknown event " + mevent.EventFlag,
                                             file.GetOffset() - 1);
            }
        }

        return result;
    }

    /** Return true if this track contains multiple channels.
 * If a MidiFile contains only one track, and it has multiple channels,
 * then we treat each channel as a separate track.
 */
    static bool HasMultipleChannels(MidiTrack track)
    {
        int channel = track.Notes[0].Channel;
        foreach (MidiNote note in track.Notes)
        {
            if (note.Channel != channel)
            {
                return true;
            }
        }
        return false;
    }

    /** Write a variable length number to the buffer at the given offset.
     * Return the number of bytes written.
     */
    static int VarlenToBytes(int num, byte[] buf, int offset)
    {
        byte b1 = (byte)((num >> 21) & 0x7F);
        byte b2 = (byte)((num >> 14) & 0x7F);
        byte b3 = (byte)((num >> 7) & 0x7F);
        byte b4 = (byte)(num & 0x7F);

        if (b1 > 0)
        {
            buf[offset] = (byte)(b1 | 0x80);
            buf[offset + 1] = (byte)(b2 | 0x80);
            buf[offset + 2] = (byte)(b3 | 0x80);
            buf[offset + 3] = b4;
            return 4;
        }
        else if (b2 > 0)
        {
            buf[offset] = (byte)(b2 | 0x80);
            buf[offset + 1] = (byte)(b3 | 0x80);
            buf[offset + 2] = b4;
            return 3;
        }
        else if (b3 > 0)
        {
            buf[offset] = (byte)(b3 | 0x80);
            buf[offset + 1] = b4;
            return 2;
        }
        else
        {
            buf[offset] = b4;
            return 1;
        }
    }

    /** Write a 4-byte integer to data[offset : offset+4] */
    private static void IntToBytes(int value, byte[] data, int offset)
    {
        data[offset] = (byte)((value >> 24) & 0xFF);
        data[offset + 1] = (byte)((value >> 16) & 0xFF);
        data[offset + 2] = (byte)((value >> 8) & 0xFF);
        data[offset + 3] = (byte)(value & 0xFF);
    }

    /** Calculate the track length (in bytes) given a list of Midi events */
    private static int GetTrackLength(List<MidiEvent> events)
    {
        int len = 0;
        byte[] buf = new byte[1024];
        foreach (MidiEvent mevent in events)
        {
            len += VarlenToBytes(mevent.DeltaTime, buf, 0);
            len += 1;  /* for eventflag */
            switch (mevent.EventFlag)
            {
                case EventNoteOn: len += 2; break;
                case EventNoteOff: len += 2; break;
                case EventKeyPressure: len += 2; break;
                case EventControlChange: len += 2; break;
                case EventProgramChange: len += 1; break;
                case EventChannelPressure: len += 1; break;
                case EventPitchBend: len += 2; break;

                case SysexEvent1:
                case SysexEvent2:
                    len += VarlenToBytes(mevent.Metalength, buf, 0);
                    len += mevent.Metalength;
                    break;
                case MetaEvent:
                    len += 1;
                    len += VarlenToBytes(mevent.Metalength, buf, 0);
                    len += mevent.Metalength;
                    break;
                default: break;
            }
        }
        return len;
    }

    /** Write the given list of Midi events into a valid Midi file. This
 *  method is used for sound playback, for creating new Midi files
 *  with the tempo, transpose, etc changed.
 *
 *  Return true on success, and false on error.
 */
    private static bool
    WriteMidiFile(string filename, List<MidiEvent>[] events, int trackmode, int quarter)
    {
        try
        {
            byte[] buf = new byte[4096];
            FileStream file = new FileStream(filename, FileMode.Create);

            /* Write the MThd, len = 6, track mode, number tracks, quarter note */
            file.Write(ASCIIEncoding.ASCII.GetBytes("MThd"), 0, 4);
            IntToBytes(6, buf, 0);
            file.Write(buf, 0, 4);
            buf[0] = (byte)(trackmode >> 8);
            buf[1] = (byte)(trackmode & 0xFF);
            file.Write(buf, 0, 2);
            buf[0] = 0;
            buf[1] = (byte)events.Length;
            file.Write(buf, 0, 2);
            buf[0] = (byte)(quarter >> 8);
            buf[1] = (byte)(quarter & 0xFF);
            file.Write(buf, 0, 2);

            foreach (List<MidiEvent> list in events)
            {
                /* Write the MTrk header and track length */
                file.Write(ASCIIEncoding.ASCII.GetBytes("MTrk"), 0, 4);
                int len = GetTrackLength(list);
                IntToBytes(len, buf, 0);
                file.Write(buf, 0, 4);

                foreach (MidiEvent mevent in list)
                {
                    int varlen = VarlenToBytes(mevent.DeltaTime, buf, 0);
                    file.Write(buf, 0, varlen);

                    if (mevent.EventFlag == SysexEvent1 ||
                        mevent.EventFlag == SysexEvent2 ||
                        mevent.EventFlag == MetaEvent)
                    {
                        buf[0] = mevent.EventFlag;
                    }
                    else
                    {
                        buf[0] = (byte)(mevent.EventFlag + mevent.Channel);
                    }
                    file.Write(buf, 0, 1);

                    if (mevent.EventFlag == EventNoteOn)
                    {
                        buf[0] = mevent.Notenumber;
                        buf[1] = mevent.Velocity;
                        file.Write(buf, 0, 2);
                    }
                    else if (mevent.EventFlag == EventNoteOff)
                    {
                        buf[0] = mevent.Notenumber;
                        buf[1] = mevent.Velocity;
                        file.Write(buf, 0, 2);
                    }
                    else if (mevent.EventFlag == EventKeyPressure)
                    {
                        buf[0] = mevent.Notenumber;
                        buf[1] = mevent.KeyPressure;
                        file.Write(buf, 0, 2);
                    }
                    else if (mevent.EventFlag == EventControlChange)
                    {
                        buf[0] = mevent.ControlNum;
                        buf[1] = mevent.ControlValue;
                        file.Write(buf, 0, 2);
                    }
                    else if (mevent.EventFlag == EventProgramChange)
                    {
                        buf[0] = mevent.Instrument;
                        file.Write(buf, 0, 1);
                    }
                    else if (mevent.EventFlag == EventChannelPressure)
                    {
                        buf[0] = mevent.ChanPressure;
                        file.Write(buf, 0, 1);
                    }
                    else if (mevent.EventFlag == EventPitchBend)
                    {
                        buf[0] = (byte)(mevent.PitchBend >> 8);
                        buf[1] = (byte)(mevent.PitchBend & 0xFF);
                        file.Write(buf, 0, 2);
                    }
                    else if (mevent.EventFlag == SysexEvent1)
                    {
                        int offset = VarlenToBytes(mevent.Metalength, buf, 0);
                        Array.Copy(mevent.Value, 0, buf, offset, mevent.Value.Length);
                        file.Write(buf, 0, offset + mevent.Value.Length);
                    }
                    else if (mevent.EventFlag == SysexEvent2)
                    {
                        int offset = VarlenToBytes(mevent.Metalength, buf, 0);
                        Array.Copy(mevent.Value, 0, buf, offset, mevent.Value.Length);
                        file.Write(buf, 0, offset + mevent.Value.Length);
                    }
                    else if (mevent.EventFlag == MetaEvent && mevent.Metaevent == MetaEventTempo)
                    {
                        buf[0] = mevent.Metaevent;
                        buf[1] = 3;
                        buf[2] = (byte)((mevent.Tempo >> 16) & 0xFF);
                        buf[3] = (byte)((mevent.Tempo >> 8) & 0xFF);
                        buf[4] = (byte)(mevent.Tempo & 0xFF);
                        file.Write(buf, 0, 5);
                    }
                    else if (mevent.EventFlag == MetaEvent)
                    {
                        buf[0] = mevent.Metaevent;
                        int offset = VarlenToBytes(mevent.Metalength, buf, 1) + 1;
                        Array.Copy(mevent.Value, 0, buf, offset, mevent.Value.Length);
                        file.Write(buf, 0, offset + mevent.Value.Length);
                    }
                }
            }
            file.Close();
            return true;
        }
        catch (IOException e)
        {
            e.ToString();
            return false;
        }
    }

    /** Clone the list of MidiEvents */
    private static List<MidiEvent>[] CloneMidiEvents(List<MidiEvent>[] origlist)
    {
        List<MidiEvent>[] newlist = new List<MidiEvent>[origlist.Length];
        for (int tracknum = 0; tracknum < origlist.Length; tracknum++)
        {
            List<MidiEvent> origevents = origlist[tracknum];
            List<MidiEvent> newevents = new List<MidiEvent>(origevents.Count);
            newlist[tracknum] = newevents;
            foreach (MidiEvent mevent in origevents)
            {
                newevents.Add(mevent.Clone());
            }
        }
        return newlist;
    }

    /** Add a tempo event to the beginning of each track */
    private static void AddTempoEvent(List<MidiEvent>[] list, int tempo)
    {
        for (int tracknum = 0; tracknum < list.Length; tracknum++)
        {

            /* Add a new tempo event to the beginning of each track */
            MidiEvent tempoEvent = new MidiEvent();
            tempoEvent.DeltaTime = 0;
            tempoEvent.StartTime = 0;
            tempoEvent.HasEventflag = true;
            tempoEvent.EventFlag = MetaEvent;
            tempoEvent.Metaevent = MetaEventTempo;
            tempoEvent.Metalength = 3;
            tempoEvent.Tempo = tempo;
            list[tracknum].Insert(0, tempoEvent);
        }
    }

    /** Start the Midi music at the given pause time (in pulses).
 *  Remove any NoteOn/NoteOff events that occur before the pause time.
 *  For other events, change the delta-time to 0 if they occur
 *  before the pause time.  Return the modified Midi Events.
 */
    private static List<MidiEvent>[]
    StartAtPauseTime(List<MidiEvent>[] list, int pauseTime)
    {
        List<MidiEvent>[] newlist = new List<MidiEvent>[list.Length];
        for (int tracknum = 0; tracknum < list.Length; tracknum++)
        {
            List<MidiEvent> events = list[tracknum];
            List<MidiEvent> newevents = new List<MidiEvent>(events.Count);
            newlist[tracknum] = newevents;

            bool foundEventAfterPause = false;
            foreach (MidiEvent mevent in events)
            {

                if (mevent.StartTime < pauseTime)
                {
                    if (mevent.EventFlag == EventNoteOn ||
                        mevent.EventFlag == EventNoteOff)
                    {

                        /* Skip NoteOn/NoteOff event */
                    }
                    else
                    {
                        mevent.DeltaTime = 0;
                        newevents.Add(mevent);
                    }
                }
                else if (!foundEventAfterPause)
                {
                    mevent.DeltaTime = (mevent.StartTime - pauseTime);
                    newevents.Add(mevent);
                    foundEventAfterPause = true;
                }
                else
                {
                    newevents.Add(mevent);
                }
            }
        }
        return newlist;
    }

    /** Change the following sound options in the Midi file:
 * - The tempo (the microseconds per pulse)
 * - The instruments per track
 * - The note number (transpose value)
 * - The tracks to include
 * Save the modified Midi data to the given filename.
 * Return true if the file was saved successfully, else false.
 */
    public bool ChangeSound(MidiSoundOptions options, string destfile)
    {
        int i;
        if (trackPerChannel)
        {
            return ChangeSoundPerChannel(options, destfile);
        }

        /* A midifile can contain tracks with notes and tracks without notes.
         * The options.tracks and options.instruments are for tracks with notes.
         * So the track numbers in 'options' may not match correctly if the
         * midi file has tracks without notes. Re-compute the instruments, and 
         * tracks to keep.
         */
        int num_tracks = events.Length;
        int[] instruments = new int[num_tracks];
        bool[] keeptracks = new bool[num_tracks];
        for (i = 0; i < num_tracks; i++)
        {
            instruments[i] = 0;
            keeptracks[i] = true;
        }
        for (int tracknum = 0; tracknum < tracks.Count; tracknum++)
        {
            MidiTrack track = tracks[tracknum];
            int realtrack = track.Number;

            instruments[realtrack] = options.instruments[tracknum];
            //keeptracks[realtrack] = options.tracks[tracknum];
        }

        List<MidiEvent>[] newevents = CloneMidiEvents(events);
        AddTempoEvent(newevents, options.tempo);

        /* Change the note number (transpose), instrument, and tempo */
        for (int tracknum = 0; tracknum < newevents.Length; tracknum++)
        {
            foreach (MidiEvent mevent in newevents[tracknum])
            {
                int num = mevent.Notenumber + options.transpose;
                if (num < 0)
                    num = 0;
                if (num > 127)
                    num = 127;
                mevent.Notenumber = (byte)num;
                if (!options.useDefaultInstruments)
                {
                    mevent.Instrument = (byte)instruments[tracknum];
                }
                mevent.Tempo = options.tempo;
            }
        }

        if (options.pauseTime != 0)
        {
            newevents = StartAtPauseTime(newevents, options.pauseTime);
        }

        /* Change the tracks to include */
        int count = 0;
        for (int tracknum = 0; tracknum < keeptracks.Length; tracknum++)
        {
            if (keeptracks[tracknum])
            {
                count++;
            }
        }
        List<MidiEvent>[] result = new List<MidiEvent>[count];
        i = 0;
        for (int tracknum = 0; tracknum < keeptracks.Length; tracknum++)
        {
            if (keeptracks[tracknum])
            {
                result[i] = newevents[tracknum];
                i++;
            }
        }

        return WriteMidiFile(destfile, result, trackmode, quarternote);
    }

    /** Change the following sound options in the Midi file:
    * - The tempo (the microseconds per pulse)
    * - The instruments per track
    * - The note number (transpose value)
    * - The tracks to include
    * Save the modified Midi data to the given filename.
    * Return true if the file was saved successfully, else false.
    *
    * This Midi file only has one actual track, but we've split that
    * into multiple fake tracks, one per channel, and displayed that
    * to the end-user.  So changing the instrument, and tracks to
    * include, is implemented differently than the ChangeSound() method:
    *
    * - We change the instrument based on the channel, not the track.
    * - We include/exclude channels, not tracks.
    * - We exclude a channel by setting the note volume/velocity to 0.
    */
    public bool ChangeSoundPerChannel(MidiSoundOptions options, string destfile)
    {
        /* Determine which channels to include/exclude.
         * Also, determine the instruments for each channel.
         */
        int[] instruments = new int[16];
        bool[] keepchannel = new bool[16];
        for (int i = 0; i < 16; i++)
        {
            instruments[i] = 0;
            keepchannel[i] = true;
        }
        for (int tracknum = 0; tracknum < tracks.Count; tracknum++)
        {
            MidiTrack track = tracks[tracknum];
            int channel = track.Notes[0].Channel;
            instruments[channel] = options.instruments[tracknum];
            //keepchannel[channel] = options.tracks[tracknum];
        }

        List<MidiEvent>[] newevents = CloneMidiEvents(events);
        AddTempoEvent(newevents, options.tempo);

        /* Change the note number (transpose), instrument, and tempo */
        for (int tracknum = 0; tracknum < newevents.Length; tracknum++)
        {
            foreach (MidiEvent mevent in newevents[tracknum])
            {
                int num = mevent.Notenumber + options.transpose;
                if (num < 0)
                    num = 0;
                if (num > 127)
                    num = 127;
                mevent.Notenumber = (byte)num;
                if (!keepchannel[mevent.Channel])
                {
                    mevent.Velocity = 0;
                }
                if (!options.useDefaultInstruments)
                {
                    mevent.Instrument = (byte)instruments[mevent.Channel];
                }
                mevent.Tempo = options.tempo;
            }
        }
        if (options.pauseTime != 0)
        {
            newevents = StartAtPauseTime(newevents, options.pauseTime);
        }
        return WriteMidiFile(destfile, newevents, trackmode, quarternote);
    }

    /** Apply the given sheet music options to the midi file.
    *  Return the midi tracks with the changes applied.
    */
    public List<MidiTrack> ChangeSheetMusicOptions(SheetMusicOptions options)
    {
        List<MidiTrack> newtracks = new List<MidiTrack>();

        for (int track = 0; track < tracks.Count; track++)
        {
            if (options.tracks[track])
            {
                newtracks.Add(tracks[track].Clone());
            }
        }

        /* To make the sheet music look nicer, we round the start times
         * so that notes close together appear as a single chord.  We
         * also extend the note durations, so that we have longer notes
         * and fewer rest symbols.
         */
        TimeSignature time = timesig;
        if (options.time != null)
        {
            time = options.time;
        }
        MidiFile.RoundStartTimes(newtracks, options.combineInterval, timesig);
        MidiFile.RoundDurations(newtracks, time.Quarter);

        if (options.twoStaffs)
        {
            newtracks = MidiFile.CombineToTwoTracks(newtracks, timesig.Measure);
        }
        if (options.shifttime != 0)
        {
            MidiFile.ShiftTime(newtracks, options.shifttime);
        }
        if (options.transpose != 0)
        {
            MidiFile.Transpose(newtracks, options.transpose);
        }

        return newtracks;
    }

    /** Shift the starttime of the notes by the given amount.
     * This is used by the Shift Notes menu to shift notes left/right.
     */
    public static void
    ShiftTime(List<MidiTrack> tracks, int amount)
    {
        foreach (MidiTrack track in tracks)
        {
            foreach (MidiNote note in track.Notes)
            {
                note.StartTime += amount;
            }
        }
    }

    /** Shift the note keys up/down by the given amount */
    public static void
    Transpose(List<MidiTrack> tracks, int amount)
    {
        foreach (MidiTrack track in tracks)
        {
            foreach (MidiNote note in track.Notes)
            {
                note.Number += amount;
                if (note.Number < 0)
                {
                    note.Number = 0;
                }
            }
        }
    }

    /* Find the highest and lowest notes that overlap this interval (starttime to endtime).
     * This method is used by SplitTrack to determine which staff (top or bottom) a note
     * should go to.
     *
     * For more accurate SplitTrack() results, we limit the interval/duration of this note 
     * (and other notes) to one measure. We care only about high/low notes that are
     * reasonably close to this note.
     */
    private static void
    FindHighLowNotes(List<MidiNote> notes, int measurelen, int startindex,
                     int starttime, int endtime, ref int high, ref int low)
    {

        int i = startindex;
        if (starttime + measurelen < endtime)
        {
            endtime = starttime + measurelen;
        }

        while (i < notes.Count && notes[i].StartTime < endtime)
        {
            if (notes[i].EndTime < starttime)
            {
                i++;
                continue;
            }
            if (notes[i].StartTime + measurelen < starttime)
            {
                i++;
                continue;
            }
            if (high < notes[i].Number)
            {
                high = notes[i].Number;
            }
            if (low > notes[i].Number)
            {
                low = notes[i].Number;
            }
            i++;
        }
    }

    /* Find the highest and lowest notes that start at this exact start time */
    private static void
    FindExactHighLowNotes(List<MidiNote> notes, int startindex, int starttime,
                          ref int high, ref int low)
    {

        int i = startindex;

        while (notes[i].StartTime < starttime)
        {
            i++;
        }

        while (i < notes.Count && notes[i].StartTime == starttime)
        {
            if (high < notes[i].Number)
            {
                high = notes[i].Number;
            }
            if (low > notes[i].Number)
            {
                low = notes[i].Number;
            }
            i++;
        }
    }

    /* Split the given MidiTrack into two tracks, top and bottom.
     * The highest notes will go into top, the lowest into bottom.
     * This function is used to split piano songs into left-hand (bottom)
     * and right-hand (top) tracks.
     */
    public static List<MidiTrack> SplitTrack(MidiTrack track, int measurelen)
    {
        List<MidiNote> notes = track.Notes;
        int count = notes.Count;

        MidiTrack top = new MidiTrack(1);
        MidiTrack bottom = new MidiTrack(2);
        List<MidiTrack> result = new List<MidiTrack>(2);
        result.Add(top); result.Add(bottom);

        if (count == 0)
            return result;

        int prevhigh = 76; /* E5, top of treble staff */
        int prevlow = 45; /* A3, bottom of bass staff */
        int startindex = 0;

        foreach (MidiNote note in notes)
        {
            int high, low, highExact, lowExact;

            int number = note.Number;
            high = low = highExact = lowExact = number;

            while (notes[startindex].EndTime < note.StartTime)
            {
                startindex++;
            }

            /* I've tried several algorithms for splitting a track in two,
             * and the one below seems to work the best:
             * - If this note is more than an octave from the high/low notes
             *   (that start exactly at this start time), choose the closest one.
             * - If this note is more than an octave from the high/low notes
             *   (in this note's time duration), choose the closest one.
             * - If the high and low notes (that start exactly at this starttime)
             *   are more than an octave apart, choose the closest note.
             * - If the high and low notes (that overlap this starttime)
             *   are more than an octave apart, choose the closest note.
             * - Else, look at the previous high/low notes that were more than an 
             *   octave apart.  Choose the closeset note.
             */
            FindHighLowNotes(notes, measurelen, startindex, note.StartTime, note.EndTime,
                             ref high, ref low);
            FindExactHighLowNotes(notes, startindex, note.StartTime,
                                  ref highExact, ref lowExact);

            if (highExact - number > 12 || number - lowExact > 12)
            {
                if (highExact - number <= number - lowExact)
                {
                    top.AddNote(note);
                }
                else
                {
                    bottom.AddNote(note);
                }
            }
            else if (high - number > 12 || number - low > 12)
            {
                if (high - number <= number - low)
                {
                    top.AddNote(note);
                }
                else
                {
                    bottom.AddNote(note);
                }
            }
            else if (highExact - lowExact > 12)
            {
                if (highExact - number <= number - lowExact)
                {
                    top.AddNote(note);
                }
                else
                {
                    bottom.AddNote(note);
                }
            }
            else if (high - low > 12)
            {
                if (high - number <= number - low)
                {
                    top.AddNote(note);
                }
                else
                {
                    bottom.AddNote(note);
                }
            }
            else
            {
                if (prevhigh - number <= number - prevlow)
                {
                    top.AddNote(note);
                }
                else
                {
                    bottom.AddNote(note);
                }
            }

            /* The prevhigh/prevlow are set to the last high/low
             * that are more than an octave apart.
             */
            if (high - low > 12)
            {
                prevhigh = high;
                prevlow = low;
            }
        }

        top.Notes.Sort(track.Notes[0]);
        bottom.Notes.Sort(track.Notes[0]);

        return result;
    }

    /** Combine the notes in the given tracks into a single MidiTrack. 
     *  The individual tracks are already sorted.  To merge them, we
     *  use a mergesort-like algorithm.
     */
    public static MidiTrack CombineToSingleTrack(List<MidiTrack> tracks)
    {
        /* Add all notes into one track */
        MidiTrack result = new MidiTrack(1);

        if (tracks.Count == 0)
        {
            return result;
        }
        else if (tracks.Count == 1)
        {
            MidiTrack track = tracks[0];
            foreach (MidiNote note in track.Notes)
            {
                result.AddNote(note);
            }
            return result;
        }

        int[] noteindex = new int[64];
        int[] notecount = new int[64];

        for (int tracknum = 0; tracknum < tracks.Count; tracknum++)
        {
            noteindex[tracknum] = 0;
            notecount[tracknum] = tracks[tracknum].Notes.Count;
        }
        MidiNote prevnote = null;
        while (true)
        {
            MidiNote lowestnote = null;
            int lowestTrack = -1;
            for (int tracknum = 0; tracknum < tracks.Count; tracknum++)
            {
                MidiTrack track = tracks[tracknum];
                if (noteindex[tracknum] >= notecount[tracknum])
                {
                    continue;
                }
                MidiNote note = track.Notes[noteindex[tracknum]];
                if (lowestnote == null)
                {
                    lowestnote = note;
                    lowestTrack = tracknum;
                }
                else if (note.StartTime < lowestnote.StartTime)
                {
                    lowestnote = note;
                    lowestTrack = tracknum;
                }
                else if (note.StartTime == lowestnote.StartTime && note.Number < lowestnote.Number)
                {
                    lowestnote = note;
                    lowestTrack = tracknum;
                }
            }
            if (lowestnote == null)
            {
                /* We've finished the merge */
                break;
            }
            noteindex[lowestTrack]++;
            if ((prevnote != null) && (prevnote.StartTime == lowestnote.StartTime) &&
                (prevnote.Number == lowestnote.Number))
            {

                /* Don't add duplicate notes, with the same start time and number */
                if (lowestnote.Duration > prevnote.Duration)
                {
                    prevnote.Duration = lowestnote.Duration;
                }
            }
            else
            {
                result.AddNote(lowestnote);
                prevnote = lowestnote;
            }
        }

        return result;
    }

    /** Combine the notes in all the tracks given into two MidiTracks,
     * and return them.
     * 
     * This function is intended for piano songs, when we want to display
     * a left-hand track and a right-hand track.  The lower notes go into 
     * the left-hand track, and the higher notes go into the right hand 
     * track.
     */
    public static List<MidiTrack> CombineToTwoTracks(List<MidiTrack> tracks, int measurelen)
    {
        MidiTrack single = CombineToSingleTrack(tracks);
        List<MidiTrack> result = SplitTrack(single, measurelen);
        return result;
    }


    /** Check that the MidiNote start times are in increasing order.
     * This is for debugging purposes.
     */
    private static void CheckStartTimes(List<MidiTrack> tracks)
    {
//        foreach (MidiTrack track in tracks)
//        {
//            int prevtime = -1;
//            foreach (MidiNote note in track.Notes)
//            {                
//                prevtime = note.StartTime;
//            }
//        }
    }

    /** In Midi Files, time is measured in pulses.  Notes that have
    * pulse times that are close together (like within 10 pulses)
    * will sound like they're the same chord.  We want to draw
    * these notes as a single chord, it makes the sheet music much
    * easier to read.  We don't want to draw notes that are close
    * together as two separate chords.
    *
    * The SymbolSpacing class only aligns notes that have exactly the same
    * start times.  Notes with slightly different start times will
    * appear in separate vertical columns.  This isn't what we want.
    * We want to align notes with approximately the same start times.
    * So, this function is used to assign the same starttime for notes
    * that are close together (timewise).
    */
    public static void
    RoundStartTimes(List<MidiTrack> tracks, int millisec, TimeSignature time)
    {
        /* Get all the starttimes in all tracks, in sorted order */
        List<int> starttimes = new List<int>();
        foreach (MidiTrack track in tracks)
        {
            foreach (MidiNote note in track.Notes)
            {
                starttimes.Add(note.StartTime);
            }
        }
        starttimes.Sort();

        /* Notes within "millisec" milliseconds apart will be combined. */
        int interval = time.Quarter * millisec * 1000 / time.Tempo;

        /* If two starttimes are within interval millisec, make them the same */
        for (int i = 0; i < starttimes.Count - 1; i++)
        {
            if (starttimes[i + 1] - starttimes[i] <= interval)
            {
                starttimes[i + 1] = starttimes[i];
            }
        }

        CheckStartTimes(tracks);

        /* Adjust the note starttimes, so that it matches one of the starttimes values */
        foreach (MidiTrack track in tracks)
        {
            int i = 0;

            foreach (MidiNote note in track.Notes)
            {
                while (i < starttimes.Count &&
                       note.StartTime - interval > starttimes[i])
                {
                    i++;
                }

                if (note.StartTime > starttimes[i] &&
                    note.StartTime - starttimes[i] <= interval)
                {

                    note.StartTime = starttimes[i];
                }
            }
            track.Notes.Sort(track.Notes[0]);
        }
    }

    /** We want note durations to span up to the next note in general.
 * The sheet music looks nicer that way.  In contrast, sheet music
 * with lots of 16th/32nd notes separated by small rests doesn't
 * look as nice.  Having nice looking sheet music is more important
 * than faithfully representing the Midi File data.
 *
 * Therefore, this function rounds the duration of MidiNotes up to
 * the next note where possible.
 */
    public static void
    RoundDurations(List<MidiTrack> tracks, int quarternote)
    {

        foreach (MidiTrack track in tracks)
        {
            for (int i = 0; i < track.Notes.Count; i++)
            {
                MidiNote note1 = track.Notes[i];

                /* Get the next note that has a different start time */
                MidiNote note2 = note1;
                for (int j = i + 1; j < track.Notes.Count; j++)
                {
                    note2 = track.Notes[j];
                    if (note1.StartTime < note2.StartTime)
                    {
                        break;
                    }
                }
                int maxduration = note2.StartTime - note1.StartTime;

                int dur = 0;
                if (quarternote <= maxduration)
                    dur = quarternote;
                else if (quarternote / 2 <= maxduration)
                    dur = quarternote / 2;
                else if (quarternote / 3 <= maxduration)
                    dur = quarternote / 3;
                else if (quarternote / 4 <= maxduration)
                    dur = quarternote / 4;


                if (dur < note1.Duration)
                {
                    dur = note1.Duration;
                }
                note1.Duration = dur;
            }
        }
    }

    /** Split the given track into multiple tracks, separating each
 * channel into a separate track.
 */
    private static List<MidiTrack>
    SplitChannels(MidiTrack origtrack, List<MidiEvent> events)
    {

        /* Find the instrument used for each channel */
        int[] channelInstruments = new int[16];
        foreach (MidiEvent mevent in events)
        {
            if (mevent.EventFlag == EventProgramChange)
            {
                channelInstruments[mevent.Channel] = mevent.Instrument;
            }
        }
        channelInstruments[9] = 128; /* Channel 9 = Percussion */

        List<MidiTrack> result = new List<MidiTrack>();
        foreach (MidiNote note in origtrack.Notes)
        {
            bool foundchannel = false;
            foreach (MidiTrack track in result)
            {
                if (note.Channel == track.Notes[0].Channel)
                {
                    foundchannel = true;
                    track.AddNote(note);
                }
            }
            if (!foundchannel)
            {
                MidiTrack track = new MidiTrack(result.Count + 1);
                track.AddNote(note);
                track.Instrument = channelInstruments[note.Channel];
                result.Add(track);
            }
        }
        return result;
    }

    /** Guess the measure length.  We assume that the measure
 * length must be between 0.5 seconds and 4 seconds.
 * Take all the note start times that fall between 0.5 and 
 * 4 seconds, and return the starttimes.
 */
    public List<int>
    GuessMeasureLength()
    {
        List<int> result = new List<int>();

        int pulses_per_second = (int)(1000000.0 / timesig.Tempo * timesig.Quarter);
        int minmeasure = pulses_per_second / 2;  /* The minimum measure length in pulses */
        int maxmeasure = pulses_per_second * 4;  /* The maximum measure length in pulses */

        /* Get the start time of the first note in the midi file. */
        int firstnote = timesig.Measure * 5;
        foreach (MidiTrack track in tracks)
        {
            if (firstnote > track.Notes[0].StartTime)
            {
                firstnote = track.Notes[0].StartTime;
            }
        }

        /* interval = 0.06 seconds, converted into pulses */
        int interval = timesig.Quarter * 60000 / timesig.Tempo;

        foreach (MidiTrack track in tracks)
        {
            int prevtime = 0;
            foreach (MidiNote note in track.Notes)
            {
                if (note.StartTime - prevtime <= interval)
                    continue;

                prevtime = note.StartTime;

                int time_from_firstnote = note.StartTime - firstnote;

                /* Round the time down to a multiple of 4 */
                time_from_firstnote = time_from_firstnote / 4 * 4;
                if (time_from_firstnote < minmeasure)
                    continue;
                if (time_from_firstnote > maxmeasure)
                    break;

                if (!result.Contains(time_from_firstnote))
                {
                    result.Add(time_from_firstnote);
                }
            }
        }
        result.Sort();
        return result;
    }

    public override string ToString()
    {
        string result = "Midi File tracks=" + tracks.Count + " quarter=" + quarternote + "\n";
        result += Time.ToString() + "\n";
        foreach (MidiTrack track in tracks)
        {
            result += track.ToString();
        }
        return result;
    }

    public static string NoteToString(int note)
	{
		string text = "";
		switch ((int)(note % 12f))
		{
			case 0:
				text = "C";
				break;
			
			case 1:
				text = "C#";
				break;
			
			case 2:
				text = "D";
				break;
			
			case 3:
				text = "D#";
				break;
			
			case 4:
				text = "E";
				break;
			
			case 5:
				text = "F";
				break;
			
			case 6:
				text = "F#";
				break;
			
			case 7:
				text = "G";
				break;
			
			case 8:
				text = "G#";
				break;
			
			case 9:
				text = "A";
				break;
			
			case 10:
				text = "A#";
				break;
			
			case 11:
				text = "B";
				break;
			
			case 12:
				text = "B#";
				break;
		}

		return string.Format("{0}{1:d}", text, (int)(note / 12f));
	}
}

[Serializable]
public class TimeSignature
{
    public enum NoteDuration
    {
        ThirtySecond,
        Sixteenth,
        Triplet,
        Eighth,
        DottedEighth,
        Quarter,
        DottedQuarter,
        Half,
        DottedHalf,
        Whole
    };

    public int numerator;      /** Numerator of the time signature */
    public int denominator;    /** Denominator of the time signature */
    public int quarternote;    /** Number of pulses per quarter note */
    public int measure;        /** Number of pulses per measure */
    public int tempo;          /** Number of microseconds per quarter note */

    /** Get the numerator of the time signature */
    public int Numerator
    {
        get { return numerator; }
    }

    /** Get the denominator of the time signature */
    public int Denominator
    {
        get { return denominator; }
    }

    /** Get the number of pulses per quarter note */
    public int Quarter
    {
        get { return quarternote; }
    }

    /** Get the number of pulses per measure */
    public int Measure
    {
        get { return measure; }
    }

    /** Get the number of microseconds per quarter note */
    public int Tempo
    {
        get { return tempo; }
    }

    /** Create a new time signature, with the given numerator,
        * denominator, pulses per quarter note, and tempo.
        */
    public TimeSignature(int numerator, int denominator, int quarternote, int tempo)
    {
        if (numerator <= 0 || denominator <= 0 || quarternote <= 0)
        {
            throw new MidiFileException("Invalid time signature", 0);
        }

        /* Midi File gives wrong time signature sometimes */
        if (numerator == 5)
        {
            numerator = 4;
        }

        this.numerator = numerator;
        this.denominator = denominator;
        this.quarternote = quarternote;
        this.tempo = tempo;

        int beat;
        if (denominator == 2)
            beat = quarternote * 2;
        else
            beat = quarternote / (denominator / 4);

        measure = numerator * beat;
    }

    /** Return which measure the given time (in pulses) belongs to. */
    public int GetMeasure(int time)
    {
        return time / measure;
    }

    /** Given a duration in pulses, return the closest note duration. */
    public NoteDuration GetNoteDuration(int duration)
    {
        int whole = quarternote * 4;

        /**
            1       = 32/32
            3/4     = 24/32
            1/2     = 16/32
            3/8     = 12/32
            1/4     =  8/32
            3/16    =  6/32
            1/8     =  4/32 =    8/64
            triplet         = 5.33/64
            1/16    =  2/32 =    4/64
            1/32    =  1/32 =    2/64
            **/

        if (duration >= 28 * whole / 32)
            return NoteDuration.Whole;
        else if (duration >= 20 * whole / 32)
            return NoteDuration.DottedHalf;
        else if (duration >= 14 * whole / 32)
            return NoteDuration.Half;
        else if (duration >= 10 * whole / 32)
            return NoteDuration.DottedQuarter;
        else if (duration >= 7 * whole / 32)
            return NoteDuration.Quarter;
        else if (duration >= 5 * whole / 32)
            return NoteDuration.DottedEighth;
        else if (duration >= 6 * whole / 64)
            return NoteDuration.Eighth;
        else if (duration >= 5 * whole / 64)
            return NoteDuration.Triplet;
        else if (duration >= 3 * whole / 64)
            return NoteDuration.Sixteenth;
        else
            return NoteDuration.ThirtySecond;
    }

    /** Convert a note duration into a stem duration.  Dotted durations
        * are converted into their non-dotted equivalents.
        */
    public NoteDuration GetStemDuration(NoteDuration dur)
    {
        if (dur == NoteDuration.DottedHalf)
            return NoteDuration.Half;
        else if (dur == NoteDuration.DottedQuarter)
            return NoteDuration.Quarter;
        else if (dur == NoteDuration.DottedEighth)
            return NoteDuration.Eighth;
        else
            return dur;
    }

    /** Return the time period (in pulses) the the given duration spans */
    public int DurationToTime(NoteDuration dur)
    {
        int eighth = quarternote / 2;
        int sixteenth = eighth / 2;

        switch (dur)
        {
            case NoteDuration.Whole:
                return quarternote * 4;
            case NoteDuration.DottedHalf:
                return quarternote * 3;
            case NoteDuration.Half:
                return quarternote * 2;
            case NoteDuration.DottedQuarter:
                return 3 * eighth;
            case NoteDuration.Quarter:
                return quarternote;
            case NoteDuration.DottedEighth:
                return 3 * sixteenth;
            case NoteDuration.Eighth:
                return eighth;
            case NoteDuration.Triplet:
                return quarternote / 3;
            case NoteDuration.Sixteenth:
                return sixteenth;
            case NoteDuration.ThirtySecond:
                return sixteenth / 2;
            default: return 0;
        }
    }

    public override string ToString()
    {
        return string.Format("TimeSignature={0}/{1} quarter={2} tempo={3}",
                                numerator, denominator, quarternote, tempo);
    }
}