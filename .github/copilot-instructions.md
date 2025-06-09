# Copilot Instructions
- Limit responses to one sentence especially for
explanations.
 
# Team Best Practices
- Always create files using file-scoped namespaces.
- When doing auth, always use .NET 8/9 idioms & auth
state is optional/should be set to false by default
- Prefer inline lambdas over full method bodies in C#.
- Prefer async and await over synchronous code.
- Never use CSS inline styles. Always use a CSS file.
- When creating records, always use constructor parameters if available.

## Testing Guidelines
- Whenever UI changes are added ensure there are
accompanying tests.
- use xUnit for unit tests
- use FluentAssertions version 7.2 for assertions
- Use Moq for mocking

