using System.Linq.Expressions;
using MyNotes.Data;

namespace MyNotes.Components.Pages;

public partial class PersonalNotes
{
    private IQueryable<PersonalNote> ApplyTextSearch(IQueryable<PersonalNote> query, string[] terms)
    {
        Expression<Func<PersonalNote, bool>>? predicate = null;
        var scope = searchScope;
        foreach (var term in terms)
        {
            var matchesAi = string.Equals(term, "AI", StringComparison.OrdinalIgnoreCase);
            Expression<Func<PersonalNote, bool>> current = n =>
                ((scope == "all" || scope == "title") && n.Title.Contains(term)) ||
                ((scope == "all" || scope == "details") && n.Details != null && n.Details.Contains(term)) ||
                ((scope == "all" || scope == "conclusion") && n.Conclusion != null && n.Conclusion.Contains(term)) ||
                ((scope == "all" || scope == "summary") && n.Summary != null && n.Summary.Contains(term)) ||
                (scope == "all" && (n.NoteGroupId != null ? n.NoteGroup!.Category : n.Category) != null && (n.NoteGroupId != null ? n.NoteGroup!.Category! : n.Category!).Contains(term)) ||
                (scope == "all" && n.NoteGroup != null && n.NoteGroup.Name.Contains(term)) ||
                ((scope == "all" || scope == "research") && n.ResearchEntries.Any(e => e.Content.Contains(term) || (e.SourceName != null && e.SourceName.Contains(term)) || (e.SourceUrl != null && e.SourceUrl.Contains(term)) || (matchesAi && e.SourceKind == ResearchSourceKind.AI)));
            if (predicate is null) predicate = current;
            else
            {
                var body = new ReplaceSearchParameter(current.Parameters[0], predicate.Parameters[0]).Visit(current.Body)!;
                predicate = Expression.Lambda<Func<PersonalNote, bool>>(matchMode == "any" ? Expression.OrElse(predicate.Body, body) : Expression.AndAlso(predicate.Body, body), predicate.Parameters);
            }
        }
        return predicate is null ? query : query.Where(predicate);
    }

    private sealed class ReplaceSearchParameter(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : base.VisitParameter(node);
    }
}
