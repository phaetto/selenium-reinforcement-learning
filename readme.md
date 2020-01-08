# Selenium Reinforcement Learning

UI tests are very flaky and they need tons of knowledge to get right and be reliable.

Partially this is because of the complexity of the underlying system.
Another reason is that UI tests are attempting to test code in a disconnected manner from the development process.
It is generally very difficult to put system tests of any kind in a branch level because of development performance issues.

This repository attempts to solve the above problems using AI (reinforcement learning) to provide an automatic way of discovering the path of actions that are need to be taken to reach a goal (the element under test).
It does that with a training process on the goal and then running the test with the trained guidance. [#TODO:Rephrase]()

## Goals

The goals of the project are:
- Create a framework that abstracts trainable steps with composite step tests
- Provide tooling that trains/replays Q-Matrices without breaking your UI tests
- Provide ordering logic for tests
- Be extremely reliable either when training or stepping forward from a matrix
- Provide an error module that can reliably provide error reporting during training
- Performance: Training should be fast as possible
- Be pipeline and DevOps process friendly

## Reinforcement Learning

The algorith used for training is Q-Learning. It creates a discreet Q Matrix using a set of predefined actions and rules to achieve a goal as fast as possible.

You can learn more about the actual algorithm at [wikipedia](https://en.wikipedia.org/wiki/Q-learning).

## Get Started

You will need to start by creating a Selenium project. Then install the package [#TODO]()
```
Selenium.ReinforcementLearning.Framework 
```

[#TODO: First test guide]()