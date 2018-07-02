# PathTree

This project was created to workaround the limitation of the FileSystemWatcher in the .NET ecosystem.

Underlying implementations vary per OS, and the main issue was the [CoreFX implementation for OS X, which uses one thread per FileSystemWatcher](https://github.com/dotnet/corefx/issues/30600).

Due to being most likely an API limitation, one workaround is to create a smarter logic that limits the number of active FileSystemWatchers by doing some path heuristics, at the expense of less glanularity of notifications.

The way the tree works is that emulates an actual file system tree, with each node representing a segment of a given path. The nodes are sorted using string comparison.

### PathTreeNode

Holds a filesystem path and a list of registration IDs for each node.

A node is considered a live node if it has a registration ID (if we want to watch `/a/b/c`, we create nodes for `a` and `b` but they shouldn't be considered as candidates to be watched.

Why this data structure?

While adding/removing paths to watch is O(logn), the advantage is that we have a pretty fast normalization, using a dfs like algorithm which stops after it yielded the requested amount of live nodes.

If we wanted to limit ourselves to have at least 8 FileSystemWatcher instances active, we could just do:

Given a tree like:
```
+ a
  + b
    + c (live)
    + d (live)
    + e (live)
    + f (live)
      + f1 (live)
      + f2 (live)
    + g (live)
      + g1 (live)
      + g2 (live)
```

Depending on how many nodes we want to watch, we have the following results for `tree.Normalize(count)` where count represents the number of watchers:
1 watcher   - `/a/b`
2 watchers  - `/a/b`
3 watchers  - `/a/b`
4 watchers  - `/a/b`
5 watchers  - `/a/b/c, /a/b/d, /a/b/e, /a/b/f, /a/b/g`
6 watchers  - `/a/b/c, /a/b/d, /a/b/e, /a/b/f, /a/b/g`
7 watchers  - `/a/b/c, /a/b/d, /a/b/e, /a/b/f, /a/b/g`
8 watchers  - `/a/b/c, /a/b/d, /a/b/e, /a/b/f, /a/b/g`

If we removed the live registrations of `f` and `g`, we would get the following results:
1 watcher   - `/a/b`
2 watchers  - `/a/b`
3 watchers  - `/a/b`
4 watchers  - `/a/b`
5 watchers  - `/a/b/c, /a/b/d, /a/b/e, /a/b/f, /a/b/g`
6 watchers  - `/a/b/c, /a/b/d, /a/b/e, /a/b/g, /a/b/f1, /a/b/f2`
7 watchers  - `/a/b/c, /a/b/d, /a/b/e, /a/b/f1, /a/b/f2, /a/b/g1, /a/b/g2`
