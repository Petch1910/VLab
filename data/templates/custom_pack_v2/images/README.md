Place optional card image files here.

Schema v2 still keeps image bytes outside the card definition hash. Use
relative paths from `pack.json` `image_root`, for example:

```text
images/sample.png
```

Then set the card row `image_file` to:

```text
sample.png
```
