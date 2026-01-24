# AgentSkillsDotNet

This is a C# Implementation of the [AgentSkills Format](https://agentskills.io/)

> This is part of [AgentFrameworkToolkit](https://github.com/rwjdk/AgentFrameworkToolkit) but can also be used on its own.

## Getting Started
1. Install the 'AgentSkillsDotNet' NuGet Package (`dotnet add package AgentSkillsDotNet`)
2. Create an `AgentSkillsFactory` instance (or use the `.AddAgentSkillsFactory` Dependency Injection)

### Minimal Code Example
```cs
AgentSkillsFactory agentSkillsFactory = new AgentSkillsFactory();
AgentSkills agentSkills = agentSkillsFactory.GetAgentSkills("<FolderWithSkillsAsSubFolders>");

//The Raw Skills
IList<AgentSkill> skills = agentSkills.Skills; 

//Get the Skills as AI Tools (to use in Microsoft Agent Framework or Microsoft.Extensions.AI)
IList<AITool> tools = agentSkills.GetAsTools(); 

//Get instructions of available skills
string instructions = agentSkills.GetInstructions(); 

//Log of skills that were excluded (due to being invalid or filtered away by advanced filtering)
IList<string> log = agentSkills.ExcludedSkillsLog; 
```

### Options when getting Skills

```cs
AgentSkillsFactory agentSkillsFactory = new AgentSkillsFactory();
AgentSkills agentSkills = agentSkillsFactory.GetAgentSkills("<FolderWithSkillsAsSubFolders>", new AgentSkillsOptions
{
    ValidationRules = AgentSkillsOptionsValidationRule.Loose, //Allow tools that don't follow Agent Skills spec 100% or use .None to turn off validation off entirely

    Filter = skill =>
    {
        //Filter what skills to include (in this example we only allow Skills that do not have any script-files)
        //But you have a full skill data available for evaluation (name, description, license, metadata, etc.)
        return skill.ScriptFiles.Length == 0;
    }
});
```

### Options for get Skills as Tools

```cs
AgentSkillsFactory agentSkillsFactory = new AgentSkillsFactory();
AgentSkills agentSkills = agentSkillsFactory.GetAgentSkills("<FolderWithSkillsAsSubFolders>");

//Expose Skills as 3 tools ('get-available-skills', 'get-skill-by-name' and 'read-skill-file-content')
IList<AITool> tools1 = agentSkills.GetAsTools(AgentSkillsAsToolsStrategy.AvailableSkillsAndLookupTools);

//Expose each skill as its own tool (+ 'read-skill-file-content' tool)
IList<AITool> tools2 = agentSkills.GetAsTools(AgentSkillsAsToolsStrategy.EachSkillAsATool);

//Control every option in the tools creation
IList<AITool> tools3 = agentSkills.GetAsTools(AgentSkillsAsToolsStrategy.AvailableSkillsAndLookupTools, new AgentSkillsAsToolsOptions
{
    IncludeToolForFileContentRead = false, //Exclude a 'read-skill-file-content' tool

    //Override default tool names/descriptions
    GetAvailableSkillToolName = "get-skills",
    GetAvailableSkillToolDescription = "Get all the skills",
    GetSpecificSkillToolName = "get-skill",
    GetSpecificSkillToolDescription = "Get a specific tool",
    ReadSkillFileContentToolName = "read-file",
    ReadSkillFileContentToolDescription = "Read a skill file",

    //Control how each Skill report it's content back (XML Structure)
    AgentSkillAsToolOptions = new AgentSkillAsToolOptions
    {
        IncludeDescription = true,
        IncludeAllowedTools = true,
        IncludeMetadata = true,
        IncludeLicenseInformation = true,
        IncludeCompatibilityInformation = true,
        IncludeScriptFilesIfAny = true,
        IncludeReferenceFilesIfAny = true,
        IncludeAssetFilesIfAny = true,
        IncludeOtherFilesIfAny = true,
    }

    /* Default Definition Example ()
        <skill name="speak-like-a-pirate" description="Let the LLM take the persona of a pirate">
            <instructions>
                # Speak Like a pirate## ObjectiveSpeak Like a pirate called 'Seadog John' ...
                He has a parrot called 'Squawkbeard'

                ## Context
                This is a persona aimed at kids that like pirates

                ## Rules
                - Use as many emojis as possible
                - As this need to be kid-friendly, do not mention alcohol and smoking
            </instructions>
       <otherFiles>
            <file>TestData\AgentSkills\speak-like-a-pirate\License.txt</file>
       </otherFiles>
       </skill>
     */
});
```

### Get Instructions about available tools

```cs
AgentSkillsFactory agentSkillsFactory = new AgentSkillsFactory();
AgentSkills agentSkills = agentSkillsFactory.GetAgentSkills("<FolderWithSkillsAsSubFolders>");

string instructions = agentSkills.GetInstructions();

/* Instructions will return in the Anthropic preferred format

<available_skills>
     <skill>
       <name>pdf-processing</name>
       <description>Extracts text and tables from PDF files, fills forms, merges documents.</description>
       <location>/path/to/skills/pdf-processing/SKILL.md</location>
     </skill>
     <skill>
       <name>data-analysis</name>
       <description>Analyzes datasets, generates charts, and creates summary reports.</description>
       <location>/path/to/skills/data-analysis/SKILL.md</location>
     </skill>
   </available_skills>
 */
```

### Work with the raw Skills

```cs
AgentSkillsFactory agentSkillsFactory = new AgentSkillsFactory();
AgentSkills agentSkills = agentSkillsFactory.GetAgentSkills("<FolderWithSkillsAsSubFolders>");

foreach (AgentSkill skill in agentSkills.Skills)
{
    //Validate the skill against the official spec
    AgentSkillValidationResult validationResult = skill.GetValidationResult();

    //Get Skill definition
    string definition = skill.GenerateDefinition(new AgentSkillAsToolOptions
    {
        //add your optional options for how the definition is generated
    });

    //Get as AI Tool
    AITool tool = skill.AsAITool(new AgentSkillAsToolOptions
    {
        //add your optional options for how the definition is generated
    });

    /* Default Definition Example
    <skill name="speak-like-a-pirate" description="Let the LLM take the persona of a pirate">
        <instructions>
            # Speak Like a pirate## ObjectiveSpeak Like a pirate called 'Seadog John' ...
            He has a parrot called 'Squawkbeard'

            ## Context
            This is a persona aimed at kids that like pirates

            ## Rules
            - Use as many emojis as possible
            - As this need to be kid-friendly, do not mention alcohol and smoking
        </instructions>
    </skill>
    */
}

```
