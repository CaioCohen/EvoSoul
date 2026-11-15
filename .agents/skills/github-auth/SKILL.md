---
name: github-auth
description: Authenticate GitHub CLI and Git operations using a user-provided environment token without exposing or persisting the secret.
---

# GitHub authentication

Use this skill only when the user has authorized a GitHub operation.

## Credential source

- Read the token from `GH_TOKEN` first, then `GITHUB_TOKEN`.
- Never print, log, echo, paste, commit, or include the token in a file, command output, URL, remote configuration, or chat response.
- If neither variable is set, stop and ask the user to authenticate with `gh auth login` or configure one of these environment variables securely.
- Do not use a token copied into project files or skill instructions.

## Operations

- Prefer the GitHub connector for repository and GitHub API operations when available.
- For GitHub CLI commands, pass the environment variable to the process without writing it to disk; verify only non-sensitive results such as the authenticated username, repository URL, visibility, or commit status.
- For Git transport, prefer an SSH remote when the user has a working SSH key; otherwise use the authenticated GitHub CLI/HTTPS flow without embedding credentials in the remote URL.
- Before external mutations, confirm the target repository, visibility, branch, and requested operation from the user's instructions.

## Secret hygiene

- Treat exposed tokens as compromised: stop using them and instruct the user to revoke and replace them.
- Never create an automation, script, or skill whose purpose is to discover arbitrary secrets or bypass authentication.
