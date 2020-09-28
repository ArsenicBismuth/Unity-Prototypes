Run below commands in a CMD, dir into this directory.
Running it using a bat file results in not saving properly on stop (Ctrl+C).

Change as necessary:

`activate py37`
`mlagents-learn Car.yaml --run-id=Car_2 --time-scale=20 --quality-level=0 --width=512 --height=512 --resume`

And run this to visualize training graphs:

`tensorboard --logdir results --port 6006`