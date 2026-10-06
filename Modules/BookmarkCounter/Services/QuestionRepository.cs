using BookmarkCounter.Data;
using Microsoft.EntityFrameworkCore;

namespace BookmarkCounter.Services;

public record QuestionRequest(string Text, string? Answer, bool IsAnswered);
public record QuestionDto(int Id, string Text, string? Answer, bool IsAnswered, DateTime CreatedAt, DateTime? AnsweredAt);

public class QuestionRepository
{
	private readonly IDbContextFactory<AppDbContext> _factory;
	public QuestionRepository(IDbContextFactory<AppDbContext> factory) => _factory = factory;

	private static QuestionDto ToDto(Question q) =>
		new(q.Id, q.Text, q.Answer, q.IsAnswered, q.CreatedAt, q.AnsweredAt);

	public async Task<List<QuestionDto>> GetAllAsync()
	{
		await using var db = await _factory.CreateDbContextAsync();
		return await db.Questions.OrderBy(q => q.IsAnswered).ThenByDescending(q => q.CreatedAt)
			.Select(q => new QuestionDto(q.Id, q.Text, q.Answer, q.IsAnswered, q.CreatedAt, q.AnsweredAt))
			.ToListAsync();
	}

	public async Task<QuestionDto> CreateAsync(QuestionRequest req)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var q = new Question { Text = req.Text.Trim(), Answer = CleanAnswer(req.Answer),
			IsAnswered = req.IsAnswered, AnsweredAt = req.IsAnswered ? DateTime.UtcNow : null };
		db.Questions.Add(q);
		await db.SaveChangesAsync();
		return ToDto(q);
	}

	public async Task<bool> UpdateAsync(int id, QuestionRequest req)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var q = await db.Questions.FindAsync(id);
		if (q is null) return false;
		q.Text = req.Text.Trim();
		q.Answer = CleanAnswer(req.Answer);
		q.AnsweredAt = req.IsAnswered ? q.AnsweredAt ?? DateTime.UtcNow : null;
		q.IsAnswered = req.IsAnswered;
		await db.SaveChangesAsync();
		return true;
	}

	public async Task<bool> DeleteAsync(int id)
	{
		await using var db = await _factory.CreateDbContextAsync();
		var q = await db.Questions.FindAsync(id);
		if (q is null) return false;
		db.Questions.Remove(q);
		await db.SaveChangesAsync();
		return true;
	}

	private static string? CleanAnswer(string? answer) => string.IsNullOrWhiteSpace(answer) ? null : answer.Trim();
}
