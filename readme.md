# Selenium Reinforcement Learning

We have to change the way we are doing testing and integrate smarter tools in
    our everyday workflows.

UI tests are hard to write, flaky, with minimum explainability and hard to maintain.
But on the other hand are easy to formulate and reason in high level.

I strongly believe that all the above can be simplified by introducing user-guided
    AI/ML algorithms in between.

## Goals

### UI Tests are hard to write
This repository attempts to solve this using AI (reinforcement learning) to
    provide an automatic way of discovering the path of actions that are need to be
    taken to reach a goal (the element under test).

It does that with a training process on a goal and then running the test with the
    trained guidance.

### Flaky tests
The best way to decrease flaky tests is to increase explainability and manage those edge cases.
This will allow to map the ones that are problems because of f.ex. network and
    retry the test until the systems around the system under test are stable.
This repo will try to provide a framework for reliable error identification to
    tackle this issue, probably involving some ML.

### Hard to maintain
UI tests needs updates correlated to almost any UI change - CSS, JS, HTML even when
    using the best practices.
This is almost eliminated due to te fact when you train a UI test, it manages to
    find each own way to the goal.
The next challenge here is to be able to find out when to train again, and how to 
    create an efficient process around that is continuous-deployment and DevOps friendly.

### In detail
In detail, the goals of the project are:
- Create a framework that abstracts trainable steps with composite step tests
- Provide tooling that trains/replays workflows without breaking your UI tests
- Provide ordering logic
- Be extremely reliable either when training or stepping forward from a matrix
- Provide an error module that can reliably provide error reporting during training
- Performance: Training should be fast as possible
- Be pipeline and DevOps process friendly

## Reinforcement Learning

The algorithm used for training is Q-Learning. It creates a discreet Q Matrix using a
    set of predefined actions and rules to achieve a goal as fast as possible.

You can learn more about the actual algorithm at
    [wikipedia](https://en.wikipedia.org/wiki/Q-learning).

### Why Q-Learning?

Q-Learning has the advantage that works really well in discrete state and action space,
    exactly what a user is doing on a page.

For this to work correctly this repo is
    ignoring the timing dimension (because that is included in the state in browser systems)
    and replaces that with "user wait" mechanic.

## Feedback

If you have any questions and/or feedback feel free to create an issue.

Also if you feel like challenging the idea and having some fun,
    feel free to send me a website and a task to train/execute! :smiley:

## License

The code is licensed under [Apache 2.0 license](LICENSE).