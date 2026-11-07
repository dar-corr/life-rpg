namespace LifeRPG.Api.Contracts.Quests;

public record CreateQuestRequest(
    string? Title,
    string? Description,
    DateTime? Deadline
);
