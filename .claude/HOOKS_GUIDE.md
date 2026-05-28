# Claude Hooks - Build & Test Verification

Claude will now automatically verify builds and tests as you work on this project.

## How It Works

Hooks are configured in `.claude/hooks/` and run automatically at specific points:

| Hook | When It Runs | What It Does |
|------|--------------|--------------|
| `post-edit` | After editing `.cs` files | Runs `dotnet build` to verify compilation |
| `post-write` | After creating new `.cs` files | Runs `dotnet build` to verify new files compile |
| `post-edit-tests` | After editing test files | Runs `dotnet test` for the specific test file |
| `pre-commit` | Before `git commit` | Runs all `dotnet test` - **blocks commit if tests fail** |

## Workflow Example

1. **Edit a file** → Build verification runs automatically
2. **Edit a test** → Only that specific test runs automatically
3. **Create new API** → Build verification runs after each file
4. **Commit changes** → All tests must pass before commit is allowed

## Quick Commands

If hooks are disabled or you want to manually verify:

```bash
# Full build
dotnet build

# Build & test
dotnet build && dotnet test

# Run only Unit Tests
dotnet test tests/UnitTests

# Run specific test
dotnet test --filter "FullyQualifiedName~PlayerServiceTests"

# Build with minimal output
dotnet build -v quiet
```

## Hook Configuration

### post-edit.json
```json
{
  "pattern": "**/*.cs",
  "command": "dotnet build --no-restore -v quiet",
  "show_output": "on_error"
}
```

- **Fast**: Uses `--no-restore` (assumes packages already restored)
- **Quiet**: Only shows output when build fails
- **Timeout**: 120 seconds

### post-edit-tests.json
```json
{
  "pattern": "tests/**/*.cs",
  "command": "dotnet test --filter 'FullyQualifiedName~{FileName}'",
  "show_output": "always"
}
```

- **Focused**: Only runs tests for the edited file
- **Fast**: Uses `--no-build` (assumes already built)
- **Always shows**: Test results are always displayed

### pre-commit.json
```json
{
  "command": "dotnet test --no-build --verbosity minimal",
  "blocking": true
}
```

- **Blocking**: Commit will be prevented if tests fail
- **Complete**: Runs all unit and integration tests
- **Timeout**: 5 minutes

## Customizing Hooks

### Disable Hooks Temporarily

Add to your shell environment:
```bash
export CLAUDE_HOOKS_ENABLED=false
```

Or create `.claude/.env`:
```
CLAUDE_HOOKS_ENABLED=false
```

### Adjust Timeout

Edit the hook JSON file and change `timeout` (in milliseconds):
```json
{
  "timeout": 300000  // 5 minutes
}
```

### Skip Build Verification

If a hook is failing due to incomplete work, you can:
1. Temporarily disable hooks
2. Complete your changes
3. Re-enable hooks
4. Fix any issues

## Troubleshooting

### "Build failed" but code looks correct
- Run `dotnet restore` first (hooks skip this for speed)
- Check for missing using statements
- Verify project references are correct

### Tests failing unexpectedly
- Run `dotnet test` manually to see full output
- Check if test dependencies are mocked correctly
- Verify test data setup (AutoFixture configuration)

### Hook taking too long
- Increase timeout in the hook JSON
- Hooks use `--no-restore` and `--no-build` flags for speed
- Consider running full test suite manually instead of auto-trigger

## File Locations

```
.claude/
├── hooks/
│   ├── README.md              # Full documentation
│   ├── post-edit.json         # Build after edit
│   ├── post-write.json        # Build after new file
│   ├── post-edit-tests.json   # Test after test edit
│   └── pre-commit.json        # Test before commit
├── settings.json              # Claude project settings
└── HOOKS_GUIDE.md             # This file
```

## Integration with Commands

Hooks work seamlessly with custom commands:

```
@create-api Transfer
```

After generating files, the hooks will:
1. Build after each file write
2. Run tests after test files are written
3. Ensure everything compiles before you're done

## Best Practices

1. **Don't disable hooks permanently** - They're there to catch errors early
2. **Fix build errors immediately** - Don't commit with broken builds
3. **Write tests for new APIs** - Use the `create-api` command template which includes tests
4. **Run full test suite** - Before major commits, run `dotnet test` manually

## Need Help?

- Check `.claude/hooks/README.md` for detailed hook documentation
- Review `CLAUDE.md` for project-specific guidelines
- Run commands manually to debug hook issues
