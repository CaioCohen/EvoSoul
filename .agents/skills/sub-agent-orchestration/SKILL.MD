This skill is to be called whenever a user asks to work on multiple items or ask to use sub-agents.

Each item from a list of item should be processed by a sub-agent with it's own context window. The sub-agent should have authority to make decisions without needing to keep asking for permission from the user (like that button with "allow once" option). The sub-agent should be able to handle the item independently and return the result back to the main agent.

also, each item should have it's own sub-agent and be processed in order (not in parallel). It means, the item 2 sub-agent will start after the sub-agent of item 1 is done.