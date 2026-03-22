---
name: issue-fetching
description: Fetch Github issue details using the Github CLI and save them to a local tickets directory. Use when user requests to fetch/download/retrieve a Github issue.
user-invocable: false
---

# Github Issue Fetcher Skill

## Description
Fetch Github issue details using the Github CLI and save them to a local tickets directory. This skill retrieves both raw JSON data and plain text format for comprehensive issue information.

## When to Use
Use this skill when:
- User requests to fetch/download/retrieve a Github issue
- User wants to save Github issue details locally
- User provides a Github issue number (e.g., ISSUE-123, PROJ-456)
- User wants to analyze or reference Github issue information

## Capabilities
- Fetches single Github issues using the Github CLI
- Retrieves up to 100 comments per issue
- Saves issue data in two formats:
  - JSON (raw format with all fields)
  - TXT (plain text format for readability)
- Organizes issues in structured directory: `issues/{ISSUE-NO}/`

## Prerequisites
- Github CLI must be installed and configured
- User must have authentication set up for Github CLI
- Access permissions to the requested Github issues

## Workflow

When a user requests a Github issue, follow these steps:

### Step 1: Validate Input
- Extract the Github issue number from user's request
- Ensure issue number follows valid Github format (e.g., ISSUE-123)
- If issue number is ambiguous, ask for clarification

### Step 2: Create Directory Structure
- Create directory: `issues/{ISSUE_NUMBER}/`
- Example: `issues/ONBOARD-1234/`

### Step 3: Fetch Raw JSON Data
Execute command:
```bash
github issue view {ISSUE_NUMBER} --raw --comments 100 > issues/{ISSUE_NUMBER}/{ISSUE_NUMBER}_raw.json
```

### Step 4: Fetch Plain Text Data
Execute command:
```bash
github issue view {ISSUE_NUMBER} --plain > issues/{ISSUE_NUMBER}/{ISSUE_NUMBER}_plain.txt
```

### Step 5: Verify Success
- Check both files were created successfully
- Verify files contain data (not empty or error messages)
- Report summary to user with file locations

### Step 6: Error Handling
If any step fails:
- Report specific error (authentication, not found, permission denied, etc.)
- Provide helpful suggestions for resolution
- Do not create empty or partial files

## Output Format
After successful execution, the directory structure will be:
```
issues/
└── {ISSUE_NUMBER}/
    ├── {ISSUE_NUMBER}_raw.json
    └── {ISSUE_NUMBER}_plain.txt
```

## Example Usage

**User Request:** "Fetch ONBOARD-1234 from Github"

**Actions:**
1. Create directory: `issues/ONBOARD-1234/`
2. Run: `github issue view ONBOARD-1234 --raw --comments 100 > issues/ONBOARD-1234/ONBOARD-1234_raw.json`
3. Run: `github issue view ONBOARD-1234 --plain > issues/ONBOARD-1234/ONBOARD-1234_plain.txt`
4. Confirm success: "✅ Fetched ONBOARD-1234 and saved to issues/ONBOARD-1234/"

## Error Messages
- **Authentication Error**: "Github CLI authentication failed. Run `github auth login` to configure."
- **Issue Not Found**: "Issue {ISSUE_NUMBER} not found. Verify the issue number is correct."
- **Permission Denied**: "Access denied to {ISSUE_NUMBER}. Check your Github permissions."
- **CLI Not Installed**: "Github CLI not found. Install from: https://github.com/cli/cli"

## Future Enhancements
- Batch fetching multiple issues
- Custom comment limits
- Filtered field extraction
- Automatic issue number detection from context
- Integration with git branch names
