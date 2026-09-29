## Git

Codex may use read-only Git commands to inspect the repository, including:

- `git status`
- `git diff`
- `git log`
- `git show`

Do not use Git commands that modify the working tree, index, branches, commits, remotes, or repository state.

Source-control operations are managed by the user outside Codex.

## Builds

Do not attempt project builds in this workspace. The .NET build currently fails before compilation because NuGet references the unavailable Windows fallback package folder `C:\Program Files (x86)\Microsoft Visual Studio\Shared\NuGetPackages`.
