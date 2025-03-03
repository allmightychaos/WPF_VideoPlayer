# WPF VideoPlayer

## Aufgabe
Erstellen Sie einen Videoplayer in WPF. Der Videoplayer soll folgende Elemente enthalten:

- Dockpanel für das generelle Layout
- MediaElement für die Videodarstellung
- Play-, Pause-, Stop-, Voriges-, Nächstes-,... Video-Steuerelemente
- Lautstärkeregler, Stummschalter
- Fortschrittsanzeige
- Eine Listbox in der die Playlist dargestellt wird
- Eine Listbox in der die PlayHistory dargestellt wird.
- Steuerlemente zum Ein-/Ausblenden der Playlist und der History
- Steuerlement um ein Video hinzuzufügen
- Steuerelement um ein Verzeichnis mit Videos hinzuzufügen
- 
Das Programm soll bei Änderung der Fenstergröße korrekt mit skalieren. Für die Playlist und die PlayHistory sollen DataTemplates für die Darstellung verwendet werden.

Den Steuerelementen (Buttons,...) sollen mit Styles und Templates ein individuelles Aussehen bekommen und passend für einen Video-Player sein.

Wenn auf ein Element in der Playlist oder der History doppelt geklickt wird soll dieses Abgespielt werden, ansonsten wird jedes mal wenn ein Video beendet ist das nächste aus der Playlist abgespielt.

Verwenden Sie [Commands](https://msdn.microsoft.com/en-us/library/system.windows.input.mediacommands(v=vs.110).aspx) wenn dies möglich ist.

[Beispiel-Video-Player](https://www.dropbox.com/s/q6yxx9g45gx4mcq/Video_Player_Beispiel.zip?dl=1)

## Hinweise
**ListBox**

Die Items in der ListBox können beliebige Objekte sein. Für die Anzeige wird ohne DataTemplate die toString Methode verwendet. Dadurch ist es möglich die ListBoxItems zu verwenden auch wenn das DataTemplate noch nicht erstellt wurde.

```cs
String vorname;
String nachname;
public override string ToString()
{
  return nachname + " " + vorname.Substring(0, 1) + ".";
}
```

**MediaElement**

Mit dem [MediaElement](http://msdn.microsoft.com/en-us/library/system.windows.controls.mediaelement.aspx) bietet WPF ein Element mit dem auf einfachste Weise Videos in ein Programm eingebunden werden kann.

XAML:

```<MediaElement Name="video" Source="C:\Users\...\VideoPlayer\Cute Cats.mp4" LoadedBehavior="Manual" />```

C#:

```cs
video.Source = new Uri("C:\Users\...\VideoPlayer\Cute Cats.mp4");
video.Play();
```

Das MediaElement kann alle Medien abspielen die auch der Windows-Media-Player abspielen kann. (z.B. mp4, avi, mpg, mpeg,...)

Damit das MediaElement im Code gesteuert werden kann muss das LoadedBehavior="Manual" gesetzt sein!

## Test Videos

Download the videos [here](https://github.com/allmightychaos/WPF_VideoPlayer/raw/master/example_videos.zip).
