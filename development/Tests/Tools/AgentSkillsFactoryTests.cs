using AgentFrameworkToolkit.Tools;
using AgentSkillsDotNet;

namespace AgentFrameworkToolkit.Tests.Tools;

public class AgentSkillsFactoryTests
{
    [Fact]
    public void AgentSkills()
    {
        AgentSkillsFactory factory = new();
        AgentSkills skills = factory.GetAgentSkills("TestData");
        Assert.NotEmpty(skills.Skills);
        Assert.Empty(skills.ExcludedSkillsLog);
    }
}
