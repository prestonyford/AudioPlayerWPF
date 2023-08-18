namespace AudioPlayerWPF.Classes
{
    public class TrackInformationConfirmEventArgs {
        public TrackInformationConfirmEventArgs(Song song) {
            Song = song;
        }
        public Song Song {
            get;
        }
    }

    public class NewPlaylistCreatedEventArgs {
        public NewPlaylistCreatedEventArgs(string name, string path, string oldName) {
            Name = name;
            Path = path;
            OldName = oldName;
        }
        public string Name {
            get;
        }
        public string Path {
            get;
        }
        public string OldName {
            get;
        }
    }
}
