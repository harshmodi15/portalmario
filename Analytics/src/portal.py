import os
import json
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
import matplotlib.image as mpimg
from matplotlib.colors import LinearSegmentedColormap

sns.set_theme(style="whitegrid")

level_name_map = {
    -1: "Basic Tutorial",
    0: "Ally Tutorial",
    1: "First Level",
    2: "Second Level"
}

level_order = ["Basic Tutorial", "Ally Tutorial", "First Level", "Second Level"]

def parse_level_data(directory: str):
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
                portal_usage = attempt_values.get("portal_usage", {})
                for _, usage in portal_usage.items():
                    rows.append({
                        "session": session_key,
                        "level": level_num,
                        "level_name": level_name_map[level_num],
                        "attempt": attempt_num,
                        "fromX": usage["fromX"],
                        "fromY": usage["fromY"],
                        "objectType": usage["objectType"],
                        "timestamp": usage["timestamp"],
                        "toX": usage["toX"],
                        "toY": usage["toY"],
                        "velocity": int(usage["velocity"])
                    })
    df = pd.DataFrame(rows)
    df['level_name'] = pd.Categorical(df['level_name'], categories=level_order, ordered=True)
    df.reset_index(drop=True, inplace=True)
    return df

def process_data(df: pd.DataFrame):
    usage_index = []
    for i, row in df.iterrows():
        if i > 0 and (row["fromX"], row["fromY"], row["toX"], row["toY"]) == (df.iloc[i-1]["fromX"], df.iloc[i-1]["fromY"], df.iloc[i-1]["toX"], df.iloc[i-1]["toY"]):
            usage_index.append(usage_index[-1])
        else:
            usage_index.append(i)
    df['usage_index'] = usage_index

    for _, group in df.groupby('usage_index'):
        if (group['velocity'] >= 10).any():
            df.loc[group.index, 'acceleration'] = 'Accelerated'
        else:
            df.loc[group.index, 'acceleration'] = 'Normal'

    df['portal_combo'] = df.apply(lambda row: f"{row['fromX']},{row['fromY']}->{row['toX']},{row['toY']}", axis=1)
    portal_counts = df.groupby(['session', 'attempt', 'portal_combo'])['fromX'].transform('count')
    df['repetitive'] = (portal_counts > 3) & (df.apply(lambda row: abs(row['toX'] - row['fromX']) > 0.75, axis=1))
    return df

def plot_portal_usage(df: pd.DataFrame):
    usage_counts = df.groupby(['level_name', 'objectType'])['usage_index'].nunique().reset_index()
    pivot = usage_counts.pivot(index='level_name', columns='objectType', values='usage_index').fillna(0)

    pastel_colors = sns.color_palette("pastel", len(pivot.columns))
    ax = pivot.plot(kind='bar', figsize=(16, 8), color=pastel_colors, edgecolor='black')

    plt.xlabel('Level', fontsize=16)
    plt.ylabel('Unique Portal Usages', fontsize=16)
    plt.title('Portal Usage by Object Type per Level', fontsize=22)
    plt.xticks(rotation=0, fontsize=14)
    plt.yticks(fontsize=14)
    plt.legend(title='Object Type', title_fontsize=16, fontsize=14, loc='upper right')
    plt.tight_layout()
    plt.show()

def plot_teleportation_types_by_usage(df: pd.DataFrame):
    teleportation_counts = df.groupby(['level_name', 'objectType'])['usage_index'].nunique().unstack(fill_value=0)
    teleportation_percent = teleportation_counts.div(teleportation_counts.sum(axis=1), axis=0) * 100

    pastel_colors = sns.color_palette("pastel", len(teleportation_percent.columns))
    ax = teleportation_percent.plot(kind='bar', stacked=True, figsize=(16, 8), color=pastel_colors, edgecolor='black')

    plt.xlabel('Level', fontsize=16)
    plt.ylabel('Percentage of Portal Usage', fontsize=16)
    plt.title('Proportional Portal Usage by Object Type per Level', fontsize=22)
    plt.xticks(rotation=0, fontsize=14)
    plt.yticks(fontsize=14)
    plt.legend(title="Object Type", title_fontsize=16, fontsize=14, loc='upper right')

    for i, level in enumerate(teleportation_percent.index):
        bottom = 0
        for obj_type in teleportation_percent.columns:
            height = teleportation_percent.loc[level, obj_type]
            if height > 0:
                if height < 10:
                    ax.text(i, bottom + height + 2, f"{int(height)}%", ha='center', va='bottom', fontsize=10)
                else:
                    ax.text(i, bottom + height / 2, f"{int(height)}%", ha='center', va='center', fontsize=10)
            bottom += height

    plt.tight_layout()
    plt.show()

def plot_teleportation_types_by_level_with_acceleration(df: pd.DataFrame):
    teleportation_counts = df.groupby(['level_name', 'acceleration'])['usage_index'].nunique().unstack(fill_value=0)

    pastel_colors = sns.color_palette("pastel", len(teleportation_counts.columns))
    ax = teleportation_counts.plot(kind='bar', stacked=True, figsize=(16, 8), color=pastel_colors, edgecolor='black')

    plt.xlabel('Level', fontsize=16)
    plt.ylabel('Teleportation Count', fontsize=16)
    plt.title('Teleportation Count (Normal vs Accelerated) per Level', fontsize=22)
    plt.xticks(rotation=0, fontsize=14)
    plt.yticks(fontsize=14)
    plt.legend(title="Teleportation Type", title_fontsize=16, fontsize=14, loc='upper right')

    for i, level in enumerate(teleportation_counts.index):
        bottom = 0
        for accel_type in teleportation_counts.columns:
            height = teleportation_counts.loc[level, accel_type]
            if height > 0:
                if height < 10:
                    ax.text(i, bottom + height + 2, f"{int(height)}", ha='center', va='bottom', fontsize=10)
                else:
                    ax.text(i, bottom + height / 2, f"{int(height)}", ha='center', va='center', fontsize=10)
            bottom += height

    plt.tight_layout()
    plt.show()

def plot_portal_heatmap(df: pd.DataFrame, level: int, img_path: str, extent: list):
    df_level = df[df["level"] == level]
    img = mpimg.imread(img_path)

    fig, ax = plt.subplots(figsize=(24, 12))
    ax.imshow(img, extent=extent, aspect='auto', alpha=1)

    colors = ["#C9A0DC", "#FF2400"]
    cmap = LinearSegmentedColormap.from_list("custom_cmap", colors, N=256)

    hb = ax.hexbin(
        df_level['fromX'],
        df_level['fromY'],
        gridsize=30,
        cmap=cmap,
        extent=extent,
        alpha=0.7,
        edgecolors='none',
        linewidth=0,
        mincnt=1
    )

    cbar = plt.colorbar(hb, ax=ax, pad=0.02)
    cbar.set_label("Portal Entry Count", fontsize=16)

    ax.set_title(f"Portal Entry Heatmap – {level_name_map[level]}", fontsize=22)
    ax.set_xlabel('posX', fontsize=16)
    ax.set_ylabel('posY', fontsize=16)
    ax.tick_params(axis='x', labelsize=14)
    ax.tick_params(axis='y', labelsize=14)

    plt.tight_layout()
    plt.show()

def plot_stuck_portals_summary(df: pd.DataFrame):
    stuck_df = df[df['repetitive']]
    stuck_counts = stuck_df.groupby("level_name")["portal_combo"].count()
    total_counts = df.groupby("level_name")["portal_combo"].count()

    ratio_df = pd.DataFrame({
        "Stuck Uses": stuck_counts,
        "Not Stuck Uses": total_counts - stuck_counts
    }).fillna(0).reindex(level_order)

    ratio_df_percent = ratio_df.div(ratio_df.sum(axis=1), axis=0) * 100

    pastel_colors = sns.color_palette("pastel", len(ratio_df.columns))

    ax = ratio_df.plot(
        kind='bar',
        stacked=True,
        figsize=(16, 12),
        color=pastel_colors,
        edgecolor='black'
    )

    plt.title("Stuck vs Total Portal Usages per Level", fontsize=22)
    plt.ylabel("Count", fontsize=16)
    plt.xlabel("Level", fontsize=16)
    plt.xticks(rotation=45, fontsize=14)
    plt.yticks(fontsize=14)
    plt.legend(
        loc='upper right',
        fontsize=14,
        title_fontsize=16,
        title="Portal Status"
    )

    for i, level in enumerate(ratio_df.index):
        bottom = 0
        for column in ratio_df.columns:
            count = ratio_df.loc[level, column]
            percent = ratio_df_percent.loc[level, column]
            if percent > 1: 
                ax.text(i, bottom + count / 2, f"{percent:.1f}%", ha='center', va='center', fontsize=13, color='black')
            bottom += count

    plt.tight_layout()
    plt.show()

if __name__ == "__main__":
    data_directory = "Analytics/Beta_Data/Beta_Details"
    df = parse_level_data(data_directory)
    df = process_data(df)

    plot_portal_usage(df)
    plot_teleportation_types_by_usage(df)
    plot_teleportation_types_by_level_with_acceleration(df)

    plot_portal_heatmap(df, -1, 'Analytics/Metrics/LevelDesignSS/tutorial_screenshot.png', extent=[-11, 97, -7, 8])
    plot_portal_heatmap(df, 0, 'Analytics/Metrics/LevelDesignSS/allyTutorial_screenshot.png', extent=[-11, 87, -6, 5])
    plot_portal_heatmap(df, 1, 'Analytics/Metrics/LevelDesignSS/lvl1_screenshot.png', extent=[-11, 91, -6.3, 7.3])
    plot_portal_heatmap(df, 2, 'Analytics/Metrics/LevelDesignSS/lvl2_screenshot.png', extent=[-18, 56, -6, 18])

    plot_stuck_portals_summary(df)