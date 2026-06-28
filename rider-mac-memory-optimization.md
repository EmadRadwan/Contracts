# Rider Memory Optimization Guide
### MacBook Pro 2017 · 16GB RAM · macOS Ventura 13

---

## 1. Diagnose — Check Memory State

**Check swap usage (run anytime to gauge memory pressure):**
```bash
sysctl vm.swapusage
```
| Result | Meaning |
|---|---|
| `used = 0` | Perfect — no swap needed |
| `used < 500MB` | Healthy |
| `used > 1GB` | Under pressure |
| `used > 1.5GB` | Critical — close apps |

**Watch swap live (refresh every 5 seconds):**
```bash
while true; do sysctl vm.swapusage; sleep 5; done
```

**Top 20 memory-hungry processes:**
```bash
ps aux | sort -k6 -rn | head -20 | awk '{printf "%6s MB  %s\n", $6/1024, $11}'
```

**Memory used by specific services:**
```bash
ps aux | grep -E "jenkins|mysql|docker|grammarly|keystone" | grep -v grep | awk '{printf "%-10s %6s MB  %s\n", $1, $6/1024, $11}'
```

**Check what's auto-starting:**
```bash
ls ~/Library/LaunchAgents/
ls /Library/LaunchDaemons/
```

**Check login items:**
```bash
osascript -e 'tell application "System Events" to get the name of every login item'
```

---

## 2. Rider JVM — Key Fix

**View current Rider VM options:**
```bash
cat ~/Library/Application\ Support/JetBrains/Rider2026.1/rider.vmoptions
```

**Optimized vmoptions (set once, keep forever):**
```
-Xms512m
-Xmx2048m
-XX:ReservedCodeCacheSize=512m
-XX:+UseG1GC
-XX:G1HeapRegionSize=32m
-XX:SoftRefLRUPolicyMSPerMB=50
-XX:MaxMetaspaceSize=768m
-Dide.mac.useNativeClipboard=true
```

**Apply via sed (if needed again):**
```bash
sed -i '' 's/-Xmx6144m/-Xmx2048m/' ~/Library/Application\ Support/JetBrains/Rider2026.1/rider.vmoptions
sed -i '' 's/-Xms1024m/-Xms512m/' ~/Library/Application\ Support/JetBrains/Rider2026.1/rider.vmoptions
```

> ⚠️ Never set `-Xmx` above 3GB on a 16GB machine — it starves the OS and causes swap.

**Enable memory indicator in Rider:**
- Right-click Rider status bar → **Memory Indicator** ✓

**Adjust heap via UI:**
- Rider → **Help → Change Memory Settings**

---

## 3. Rider Plugins — Disable to Save RAM

| Plugin | Action | Reason |
|---|---|---|
| AI Assistant | ❌ Disable | Always running, high RAM |
| Junie | ❌ Disable | Always running, ~100MB idle |
| dotTrace | ✅ Keep | Dormant until used |
| dotMemory | ✅ Keep | Dormant until used |
| MCP Server | ✅ Keep | Enables Claude CLI integration |
| Qodana | ✅ Keep | Disable auto-scan, run manually |

**Verify Junie is not running after disabling:**
```bash
ps aux | grep -i junie | grep -v grep
```

---

## 4. Startup Services — Removed

| Service | Memory Saved | Command Used |
|---|---|---|
| Jenkins | ~122 MB | `brew services stop jenkins && brew uninstall jenkins` |
| Homebrew MySQL (duplicate) | ~344 MB | `brew services stop mysql && brew services stop mysql@8.0` |
| **Total saved** | **~466 MB** | |

> ✅ Oracle MySQL at `/usr/local/mysql/` kept — this is the active one used by the app.

**Check running brew services:**
```bash
brew services list
```

**Stop a service (reversible):**
```bash
brew services stop <service-name>
```

**Start a service when needed:**
```bash
brew services start <service-name>
```

---

## 5. Chrome — Keep Lean

**Count Chrome processes:**
```bash
ps aux | grep -i "Google Chrome" | grep -v grep | wc -l
```

**See Chrome memory per process:**
```bash
ps aux | grep -i "Google Chrome" | grep -v grep | awk '{printf "%6.1f MB  %s %s %s\n", $6/1024, $11, $12, $13}' | sort -rn
```

**Rules while coding:**
- Max 5 tabs while Rider is open
- Use native apps instead of browser tabs (WhatsApp, Mail)
- Enable Memory Saver: Chrome → Settings → Performance → Memory Saver → On
- Disable extensions not needed while coding (Grammarly especially)

**Disable Chrome dynamic color (stops toolbar color changes):**
- Go to `chrome://flags` → search **dynamic color** → Disabled → Relaunch

**See tab list with memory info:**
- Go to `chrome://discards`

---

## 6. Toolbox — Free Rider from Toolbox Management

If Toolbox stops working, detach Rider so it updates independently:

```bash
sed -i '' '/-Dide.managed.by.toolbox/d' ~/Library/Application\ Support/JetBrains/Rider2026.1/rider.vmoptions
sed -i '' '/-Dtoolbox.notification.token/d' ~/Library/Application\ Support/JetBrains/Rider2026.1/rider.vmoptions
```

After restart: **Rider → Help → Check for Updates** works independently.

---

## 7. Spotlight — Exclude Dev Directories

Prevents background indexing from spiking CPU/memory during coding:

- **System Settings → Spotlight → Privacy**
- Add: your project folders, `node_modules`, `build`, `dist`

---

## 8. Quick Health Check Routine

Run this anytime Rider feels sluggish:

```bash
# 1. Check swap
sysctl vm.swapusage

# 2. Find memory hogs
ps aux | sort -k6 -rn | head -10 | awk '{printf "%6s MB  %s\n", $6/1024, $11}'

# 3. Check Chrome process count
ps aux | grep -i "Google Chrome" | grep -v grep | wc -l
```

**If swap > 1GB:** close Chrome tabs, quit unused apps, restart Rider.

---

## 9. Expected Memory Budget (Optimized)

| Component | Memory |
|---|---|
| Rider (capped at 2GB) | ~1,500 MB |
| Rider backend + dotnet | ~1,000 MB |
| Chrome (2–5 tabs) | ~400 MB |
| Your API + Node dev server | ~350 MB |
| macOS overhead | ~1,500 MB |
| **Total** | **~4,750 MB** |

Leaves ~11GB headroom on a 16GB machine — no swap pressure under normal conditions.

---

*Optimized: June 2026 · MacBook Pro 2017 · macOS Ventura 13.7.8 · Rider 2026.1*
