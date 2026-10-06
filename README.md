# Minipaso — みんなのパソコン tribute

**Project page: https://colingamez.github.io/minipaso/** (live leaderboard + downloads)

A Windows-only successor to GOGA's legendary Japan-exclusive **Minpaso Gadget**
(みんなのパソコン, 2007) for the Vista Sidebar. Same idea, modernized:

1. **測定 Measure** — reads your Windows Experience Index from
   `C:\Windows\Performance\WinSAT\DataStore` (or runs `winsat formal` for you).
2. **投票 Vote** — submits your scores to the leaderboard with a nickname you choose.
3. **ランキング Ranking** — opens the live leaderboard on ColinGamez's website.

Built era-authentic: **C# 4, .NET Framework 4.0, WinForms**. Opens and compiles in
**Visual Studio 2010** with zero extensions.

## Privacy (better than 2007)

The original gadget uploaded your drive serial numbers and volume labels along
with the scores. This version sends only: the six WEI subscores, PC manufacturer
+ model (from WMI), your nickname, and a timestamp. No serials, no drive contents.
The leaderboard lives on Cloudflare R2; see the website repo's `r2-worker.js`.

## Build

Open `Minipaso.sln` in Visual Studio 2010 and press F5. That's it.

Before voting works, point the app at your worker in `src/Reporter.cs`:

```csharp
private const string WorkerUrl = "https://your-worker.workers.dev";
```

## Layout

- `src/Program.cs` — entry point
- `src/MainForm.cs` — the window (measure / vote / ranking + score grid)
- `src/WinSat.cs` — reads WinSAT DataStore XML, can launch `winsat formal`
- `src/Reporter.cs` — POSTs your score XML to the worker, opens the ranking page
