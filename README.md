PortalMario is a custom 2D Mario-style platformer with embedded gameplay analytics to study player behavior, difficulty tuning, and level design efficiency.

This project demonstrates the intersection of **game development, player telemetry, and data science**, enabling insights into how players interact with the environment through:

---

## 🎯 Key Features

- 🕹️ Fully playable custom-built 2D platformer
- 🔥 **Heatmap visualization** of player movement and death locations
- 🧠 Player behavior analytics using Python (Pandas, Matplotlib, Seaborn)
- 📡 Telemetry logging powered by Firebase / custom event tracking
- 📈 Insights used for game difficulty tuning and UX evaluation

---

## 🧱 Tech Stack

| Component | Technology |
|----------|------------|
| Game Build | Unity / C# |
| Analytics | Python |
| Visualization | Heatmaps, Matplotlib |
| Telemetry | Firebase / CSV event logs |

---

## 📦 Project Structure

```
├── Game
   ├── Assets/                # Unity assets, sprites, scenes
   ├── Scripts/               # C# scripts controlling gameplay
   ├── Firebase/ (ignored)    # Sensitive config
├── Analytics/             # Python notebooks + visualization scripts
├── requirements.txt       # Python dependencies (analytics only)
└── README.md
```

> Note: Event logs and environment-specific configs are intentionally excluded for privacy and storage optimization.

---

## 🚀 Future Enhancements

- [ ] Upload gameplay video demo
- [ ] Add architecture diagram (C# + analytics pipeline)
- [ ] Add WebGL playable export
- [ ] Include example heatmap output

---

## 📜 License

MIT License.

---
