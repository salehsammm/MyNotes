using System.Text.Json.Nodes;

namespace BookmarkCounter.Services;

public class BookmarkReader
{
	private readonly string _path;

	public BookmarkReader(string path) => _path = path;

	public int CountFolder(string folderName)
	{
		if (!File.Exists(_path)) return -1;

		var json = File.ReadAllText(_path);
		var root = JsonNode.Parse(json);
		var roots = root?["roots"];
		if (roots is null) return -1;

		foreach (var rootName in new[] { "bookmark_bar", "other", "synced" })
		{
			var found = FindFolder(roots[rootName], folderName);
			if (found is not null) return CountUrls(found);
		}
		return 0;
	}

	private static JsonNode? FindFolder(JsonNode? node, string name)
	{
		if (node is null) return null;

		if (node["type"]?.GetValue<string>() == "folder" &&
			node["name"]?.GetValue<string>() == name)
			return node;

		if (node["children"] is JsonArray children)
		{
			foreach (var child in children)
			{
				var found = FindFolder(child, name);
				if (found is not null) return found;
			}
		}
		return null;
	}

	private static int CountUrls(JsonNode? node)
	{
		if (node is null) return 0;
		if (node["type"]?.GetValue<string>() == "url") return 1;

		var count = 0;
		if (node["children"] is JsonArray children)
		{
			foreach (var child in children)
				count += CountUrls(child);
		}
		return count;
	}
}