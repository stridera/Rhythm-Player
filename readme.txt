Rhythm Player - This is the code behind an arduino based Rhythm game (Rock Band, Guitar Hero, etc) automatic player.  The code consists of 3 projects:

Rhythm Player Midi - This code is designed to read the midi files from the disk and create an intermediary file displays the notes and the delay between each note.  The code was never completed to a 'releasable' stage, but was completed enough just to work for our project.  Changes will need to be made to get it to read different games formats.  

Rhythm Player Client - This code reads the intermediate file that was created by the Rhythm Player Midi project and sends the notes to the arduino to be played.  Again, some tweaking will be required to get it to work in different modes.

Rhythm Player Arduino – The arduino code that listens to the notes via the serial port and sends the notes to the modified guitars.

All projects were simply hacked to get it to work under different situations.  (Single Player, Co-op, multiple player, etc.)  If people are actually interested, I’ll take time to make it ‘release’ worthy.  (Add drop down selection to change to the different modes, etc.


