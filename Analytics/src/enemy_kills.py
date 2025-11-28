"""
enemy_kills.py
==================
Analyzes and visualizes enemy kill data in the Portal Mario 2D platformer game.

🎯 Objective:
-------------
This script processes playtesting data collected from Firebase for each level of the game.
It visualizes *where* and *why* enemies were killed, allowing designers to study
player strategy, level difficulty, and mechanic balance.

The heatmaps overlay kill events on level screenshots for contextual understanding.
Different kill reasons (e.g., "Box", "Fall", "Accelerated Box") are shown
with unique markers, colors, and vertical offsets for clarity.

📊 Outputs:
-----------
1. **Bar Chart** – Enemy kill reasons per level.
2. **Heatmaps** – Spatial distribution of kill events across levels.

🧰 Dependencies:
----------------
- Python 3.x
- pandas, matplotlib, json, os

📁 Input Folder:
----------------
../Beta/Beta_Details/
Contains JSON gameplay data files for each level (e.g., `level_0.json`).
"""

import os
import json
import matplotlib.pyplot as plt
import pandas as pd
import matplotlib.image as mpimg
import matplotlib.lines as mlines


# --- Basic setup: level names, screenshot mapping, and colors/markers ---

level_name_map = {
    -1: "Basic Tutorial",
    0: "Ally Tutorial",
    1: "First Level",
    2: "Second Level"
}
level_order = ["Basic Tutorial", "Ally Tutorial", "First Level", "Second Level"]

# Screenshots for heatmaps
level_screenshots = {
    -1: ("../Metrics/LevelDesignSS/tutorial_screenshot.png", [-11, 97, -7, 8]),
    0: ("../Metrics/LevelDesignSS/allyTutorial_screenshot.png", [-11, 86, -6, 5]),
    1: ("../Metrics/LevelDesignSS/lvl1_screenshot.png", [-11, 91, -6.3, 7.3]),
    2: ("../Metrics/LevelDesignSS/lvl2_screenshot.png", [-18, 56, -6, 18])
}

plot_colors = [
    "#7f7f7f", "#2ca02c", "#bcbd22", "#17becf", "#ce9340",
    "#f123aa", "#1f77b4", "#ff7f0e", "#d62728", "#926e38"
]
plot_markers = ['<', 'v', 'D', 'o', '*', 'X', '^', 'P', '2', 's']


# --- Step 1: Read JSON data and extract kill events ---

def parse_data(directory: str) -> pd.DataFrame:
    rows = []
    for filename in os.listdir(directory):
        if not filename.startswith("level_") or not filename.endswith(".json"):
            continue

        level_num = int(filename.split("_")[1].split(".")[0])
        file_path = os.path.join(directory, filename)
        with open(file_path, 'r') as f:
            data = json.load(f)

        for session_key, session_values in data.items():
            for attempt_key, attempt_values in session_values.items():
                attempt_num = int(attempt_key.split("_")[1])
                enemy_kills = attempt_values.get("enemy_kills", {})

                for _, kills in enemy_kills.items():
                    rows.append({
                        "session": session_key,
                        "level": level_num,
                        "level_name": level_name_map[level_num],
                        "attempt": attempt_num,
                        "posX": kills["posX"],
                        "posY": kills["posY"],
                        "reason": kills["reason"],
                        "timestamp": kills["timestamp"],
                    })

    df = pd.DataFrame(rows)
    df['level_name'] = pd.Categorical(df['level_name'], categories=level_order, ordered=True)
    df.reset_index(drop=True, inplace=True)
    return df


# --- Step 2: Clean up inconsistent labels or duplicates ---

def process_data(df: pd.DataFrame) -> pd.DataFrame:
    df = df[~df['reason'].str.contains(r'#1|#2', na=False)]
    df.loc[:, 'reason'] = df['reason'].str.replace('#3', '3 times', regex=False)
    return df


# --- Step 3: Assign consistent colors and markers ---

def create_reason_mappings(df: pd.DataFrame):
    unique_reasons = df['reason'].unique()
    color_map, marker_map = {}, {}
    for idx, reason in enumerate(unique_reasons):
        color_map[reason] = plot_colors[idx % len(plot_colors)]
        marker_map[reason] = plot_markers[idx % len(plot_markers)]
    return color_map, marker_map


# --- Step 4: Bar Chart – Enemy Kill Reasons per Level ---

def plot_reason_counts(df: pd.DataFrame, color_map: dict):
    reason_counts = (df.groupby(['level_name', 'reason'], observed=False).size().unstack(fill_value=0))
    total_reason_counts = df['reason'].value_counts().to_dict()

    fig, ax = plt.subplots(figsize=(18, 10))
    reason_counts.plot(kind='bar', ax=ax, color=[color_map.get(r, 'gray') for r in reason_counts.columns])

    ax.set_xlabel('Level', fontsize=16)
    ax.set_ylabel('Number of Enemy Kills', fontsize=16)
    ax.set_title('Enemy Kill Reasons per Level', fontsize=20)
    ax.grid(True, axis='y', linestyle='--', alpha=0.3)
    plt.xticks(rotation=0)

    for container in ax.containers:
        ax.bar_label(container, labels=[int(v) if v > 0 else '' for v in container.datavalues],
                     label_type='edge', fontsize=12, padding=3)

    legend_labels = [f"{r} ({total_reason_counts.get(r, 0)})" for r in reason_counts.columns]
    ax.legend(labels=legend_labels, loc='upper center', bbox_to_anchor=(0.5, -0.12),
              ncol=min(len(legend_labels), 5), title='Reason (Total Count)',
              fontsize=12, title_fontsize=14, framealpha=0.8)

    plt.tight_layout(pad=1.5)
    plt.show()


# --- Step 5: Heatmaps per Level ---

def plot_kill_positions(df: pd.DataFrame, level: int, image_path: str, extent: list, color_map, marker_map):
    img = mpimg.imread(image_path)
    fig, ax = plt.subplots(figsize=(24, 10))

    level_name = level_name_map[level]
    df_level = df[df["level"] == level].copy()

    # 🔧 Custom offsets you can tune manually after viewing each visualization
    # Set everything to 0.0 initially — adjust as needed once you inspect visuals
    custom_offsets = {
        "Player": 0.7,
        "Player 3 times": 0.8,
        "Converted to Ally": -0.1,
        "Laser": 1.0,
        "Accelerated Box": 1.1,
        "Box": 1.3,
        "Ally": 1.5,
        "Ally Killed": 0.0,
        "Fall": -3.0,
        "Box 3 times": 1.2
    }

    # Apply offsets safely (default 0.0 if not defined)
    df_level["posY_offset"] = df_level.apply(
        lambda row: row["posY"] + custom_offsets.get(row["reason"], 0.0), axis=1
    )

    ax.imshow(img, extent=extent, aspect='auto', alpha=0.6)
    reason_counts = df_level['reason'].value_counts().to_dict()

    for reason in sorted(df_level["reason"].unique()):
        subset = df_level[df_level["reason"] == reason]
        marker = marker_map.get(reason, 'o')
        ax.scatter(
            subset["posX"], subset["posY_offset"],
            color=color_map.get(reason, 'black'),
            marker=marker,
            label=f"{reason} ({reason_counts.get(reason, 0)})",
            s=180, edgecolors='black', linewidths=0.8, alpha=0.85
        )

    ax.set_xlim(extent[0], extent[1])
    ax.set_ylim(extent[2], extent[3])
    ax.set_title(f'Enemy Kill Positions – {level_name}', fontsize=22)
    ax.set_xlabel('posX (Level Coordinate)', fontsize=18)
    ax.set_ylabel('posY (Adjusted for Mechanics)', fontsize=18)
    ax.grid(True, linestyle='--', alpha=0.3, linewidth=0.5)

    legend_items = [
        mlines.Line2D([], [], color=color_map.get(r, 'black'), marker=marker_map.get(r, 'o'),
                      linestyle='None', markersize=18, label=f"{r} ({reason_counts.get(r, 0)})",
                      markeredgecolor='black', alpha=0.8)
        for r in sorted(df_level["reason"].unique())
    ]
    ax.legend(handles=legend_items, loc='upper center', bbox_to_anchor=(0.5, -0.1),
              ncol=min(len(legend_items), 5), title='Reason (Count)',
              title_fontsize=18, fontsize=16, framealpha=0.8)

    plt.tight_layout(pad=1.5)
    plt.show()


# --- Step 6: Run the full analysis ---

if __name__ == "__main__":
    data_directory = "../Beta/Beta_Details"

    df = parse_data(data_directory)
    df = process_data(df)
    color_map, marker_map = create_reason_mappings(df)

    plot_reason_counts(df, color_map)

    for level_id, (img_path, extent) in level_screenshots.items():
        plot_kill_positions(df, level_id, img_path, extent, color_map, marker_map)


"""
Why this visualization matters:
-------------------------------
Enemy kill heatmaps help designers see how players interact with each mechanic.
If many kills cluster near certain obstacles or enemies, it can reveal unbalanced
difficulty or overpowered tools (like the portal gun or box). The goal is to make
combat and level flow feel intuitive, not accidental.
"""