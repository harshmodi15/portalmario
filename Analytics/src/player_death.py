import os
import json
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import matplotlib.image as mpimg
import matplotlib.lines as mlines

sns.set_theme(style="whitegrid")

data_dir = "Analytics/Beta_Data/Beta_Details"
json_files = {-1: "level_-1.json", 0: "level_0.json", 1: "level_1.json", 2: "level_2.json"}
level_name_map = {-1: "Basic Tutorial", 0: "Ally Tutorial", 1: "First Level", 2: "Second Level"}
level_order = ["Basic Tutorial", "Ally Tutorial", "First Level", "Second Level"]
level_screenshots = {
    -1: ("Analytics/Metrics/LevelDesignSS/tutorial_screenshot.png", [-11, 97, -7, 8]),
    0: ("Analytics/Metrics/LevelDesignSS/allyTutorial_screenshot.png", [-11, 87, -6, 5]),
    1: ("Analytics/Metrics/LevelDesignSS/lvl1_screenshot.png", [-11, 91, -6.3, 7.3]),
    2: ("Analytics/Metrics/LevelDesignSS/lvl2_screenshot.png", [-18, 56, -6, 18])
}

rows = []
for level, file in json_files.items():
    path = os.path.join(data_dir, file)
    with open(path, "r") as f:
        data = json.load(f)
    for player_id, attempts in data.items():
        for attempt_key, metrics in attempts.items():
            for death in metrics.get("deathReasons", {}).values():
                rows.append({
                    "player_id": player_id,
                    "attempt": attempt_key,
                    "level": level,
                    "level_name": level_name_map[level],
                    "posX": death["posX"],
                    "posY": death["posY"],
                    "timestamp": pd.to_numeric(death["timestamp"], errors="coerce"),
                    "reason": death["reason"]
                })

df_death = pd.DataFrame(rows)
df_death["level_name"] = pd.Categorical(df_death["level_name"], categories=level_order, ordered=True)
unique_reasons = df_death["reason"].unique()

for level_id, (_, extent) in level_screenshots.items():
    ymin = extent[2]
    mask = (df_death["level"] == level_id) & (df_death["reason"] == "Fall")
    df_death.loc[mask, "posY"] = ymin + 1

top5_reasons = df_death["reason"].value_counts().index[:5]
bar_colors_list = ['#FFD700', '#7F7F7F', '#17BECF', '#8B4513', '#00CED1']
marker_styles = ['o', 's', 'D', 'X', 'P']
reason_color_map = {reason: bar_colors_list[i] for i, reason in enumerate(top5_reasons)}
reason_marker_map = {reason: marker_styles[i] for i, reason in enumerate(top5_reasons)}

fig, axes = plt.subplots(2, 2, figsize=(16, 12))
axes = axes.flatten()
for i, level in enumerate(level_order):
    level_data = df_death[df_death["level_name"] == level]
    reason_counts = level_data["reason"].value_counts()
    reasons = reason_counts.index
    colors = [reason_color_map.get(reason, '#7F7F7F') for reason in reasons]
    wedges, texts, autotexts = axes[i].pie(
        reason_counts,
        labels=reasons,
        autopct='%1.1f%%',
        startangle=90,
        colors=colors,
        textprops={'fontsize': 14}
    )
    axes[i].set_title(f"Death Reasons – {level}", fontsize=18)
plt.suptitle("Death Reason Distribution per Level", fontsize=24)
plt.tight_layout()
plt.subplots_adjust(top=0.9)
plt.show()

fig, axes = plt.subplots(2, 2, figsize=(18, 14))
axes = axes.flatten()

for idx, level in enumerate(level_order):
    level_data = df_death[df_death["level_name"] == level]
    players_per_reason = level_data.groupby("reason")["player_id"].nunique().sort_values(ascending=False)
    top_reasons = players_per_reason.index[:5]
    top_counts = players_per_reason.values[:5]
    colors_to_use = [reason_color_map.get(reason, '#7F7F7F') for reason in top_reasons]

    axes[idx].bar(
        top_reasons,
        top_counts,
        color=colors_to_use,
        edgecolor='black'
    )
    axes[idx].set_title(f"Players Affected per Death Reason – {level}", fontsize=20)
    axes[idx].set_xlabel("Reason", fontsize=16)
    axes[idx].set_ylabel("Unique Players", fontsize=16)
    axes[idx].tick_params(axis='x', labelsize=14, rotation=45)
    axes[idx].tick_params(axis='y', labelsize=14)

custom_legend_handles = [
    mlines.Line2D([], [], 
                color=bar_colors_list[i],
                marker=marker_styles[i],
                linestyle='None',
                markersize=14,
                label=f"{top5_reasons[i]}",
                markeredgecolor='black')
    for i in range(len(top5_reasons))
]

fig.legend(
    handles=custom_legend_handles,
    loc='upper center',
    bbox_to_anchor=(0.5, -0.08),
    ncol=5,
    fontsize=14,
    title="Top 5 Death Reasons",
    title_fontsize=16,
    framealpha=0.8,
    borderpad=1.2,
    labelspacing=0.8,
    handletextpad=1.0
)

plt.tight_layout()
plt.subplots_adjust(top=0.9)
plt.show()

player_death_counts = df_death.groupby(["level_name", "player_id"]).size().reset_index(name="death_count")
fig, axes = plt.subplots(2, 2, figsize=(16, 12))
axes = axes.flatten()
for i, level in enumerate(level_order):
    data = player_death_counts[player_death_counts["level_name"] == level]
    avg_death = data["death_count"].mean()
    if not data.empty:
        bins = range(1, data["death_count"].max() + 2)
        axes[i].hist(
            data["death_count"],
            bins=bins,
            color='purple',
            edgecolor='black',
            align='left',
            rwidth=0.9
        )
        axes[i].set_title(f"Player Death Count – {level}", fontsize=18)
        axes[i].set_xlabel("Deaths per Player", fontsize=14)
        axes[i].set_ylabel("Player Count", fontsize=14)
        axes[i].legend([f"Avg: {avg_death:.1f}"], loc="upper right", fontsize=12, frameon=True)
plt.tight_layout()
plt.show()

for level, (screenshot_path, extent) in level_screenshots.items():
    fig, ax = plt.subplots(figsize=(24, 12))
    img = mpimg.imread(screenshot_path)
    ax.imshow(img, extent=extent, aspect='auto', alpha=0.9)

    for reason in unique_reasons:
        data = df_death[(df_death["level"] == level) & (df_death["reason"] == reason)]
        ax.scatter(
            data["posX"],
            data["posY"],
            label=f"{reason} ({len(data)})",
            s=150,
            color=reason_color_map.get(reason, 'gray'),
            marker=reason_marker_map.get(reason, 'o'),
            edgecolors='black',
            linewidths=0.5,
            alpha=0.9
        )

    ax.set_title(f"Death Heatmap – {level_name_map[level]}", fontsize=22)
    ax.set_xlim(extent[0], extent[1])
    ax.set_ylim(extent[2], extent[3])
    ax.set_xlabel("posX", fontsize=18)
    ax.set_ylabel("posY", fontsize=18)
    ax.tick_params(axis='x', labelsize=14)
    ax.tick_params(axis='y', labelsize=14)

    ax.legend(
        loc='upper right',
        title='Death Reason (Count)',
        title_fontsize=16,
        fontsize=14,
        framealpha=0.8,
        borderpad=1.2,
        labelspacing=0.8,
        handletextpad=1.0
    )

    plt.grid(False)
    plt.tight_layout()
    plt.show()