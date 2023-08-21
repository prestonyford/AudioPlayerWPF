# AudioPlayerWPF
An audio player built with .NET's WPF framework that provides several useful features, such as editing tag properties, playlists, bookmarks, and other individual song settings.

Required Dependencies (NuGet):
- TagLib#

Some things to note:
- I created a custom linked list class so that removing and inserting songs into a playlist at any position can be done in constant time. The reason I made my own class instead of using the linked list implementation in the BCL is because I needed to enumerate over nodes instead of values. In the future though, I would probably just use an array since it likely would still be faster for reasonable sized playlists.
