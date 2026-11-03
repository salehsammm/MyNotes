namespace MovieReviews.Data;

public class Playlist
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<PlaylistScene> Scenes { get; set; } = new();
}

public class PlaylistScene
{
    public int PlaylistId { get; set; }
    public Playlist Playlist { get; set; } = null!;
    public int SceneId { get; set; }
    public Scene Scene { get; set; } = null!;
}
