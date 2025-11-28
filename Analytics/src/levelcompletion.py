import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import os

sns.set_theme(style="whitegrid")

file_paths = {
    "-1": "Analytics/Beta_Data/Beta_Overview/level_-1.csv",
    "0": "Analytics/Beta_Data/Beta_Overview/level_0.csv",
    "1": "Analytics/Beta_Data/Beta_Overview/level_1.csv",
    "2": "Analytics/Beta_Data/Beta_Overview/level_2.csv"
}

level_name_map = {
    "-1": "Basic Tutorial",
    "0": "Ally Tutorial",
    "1": "First Level",
    "2": "Second Level"
}

level_order = ["Basic Tutorial", "Ally Tutorial", "First Level", "Second Level"]

def label_time_bins(t):
    if pd.isna(t): return 'Unknown'
    if t < 40: return '<40s'
    elif t < 60: return '40–60s'
    elif t < 80: return '60–80s'
    elif t < 100: return '80–100s'
    elif t < 100: return '100–140s'
    elif t < 100: return '140–180s'
    else: return '180s+'

time_range_order = ['<40s', '40–60s', '60–80s', '80–100s', '100–140s', '140–180s', '180s+']

data_frames = []
for level_num, path in file_paths.items():
    if os.path.exists(path):
        df = pd.read_csv(path)
        df["level"] = level_num
        df["level_name"] = level_name_map[level_num]
        data_frames.append(df)

df_all = pd.concat(data_frames, ignore_index=True)
df_all["completionTime"] = pd.to_numeric(df_all["completionTime"], errors='coerce')
df_all["completed"] = df_all["completed"].astype(bool)
df_all["level_name"] = pd.Categorical(df_all["level_name"], categories=level_order, ordered=True)
df_all["deaths"] = pd.to_numeric(df_all["deaths"], errors="coerce")
df_all["retries"] = pd.to_numeric(df_all["retries"], errors="coerce")

avg_deaths = df_all.groupby("level_name")["deaths"].mean().reindex(level_order)
completion_rate = (
    df_all[df_all["completed"]].groupby("level_name")["player_id"].nunique() /
    df_all.groupby("level_name")["player_id"].nunique()
).reindex(level_order)

fig, ax1 = plt.subplots(figsize=(14, 8))
bars = ax1.bar(avg_deaths.index, avg_deaths.values, color=sns.color_palette("Set2"), edgecolor="black", label='Avg Deaths')

ax2 = ax1.twinx()
ax2.plot(completion_rate.index, completion_rate.values * 100, color='darkorange', marker='o', markersize=8, linewidth=2, label='Completion Rate (%)')

ax1.set_ylabel('Average Deaths', fontsize=16)
ax2.set_ylabel('Completion Rate (%)', fontsize=16)
ax1.set_xlabel('Level', fontsize=16)
ax1.set_title("Average Deaths & Completion Rate by Level", fontsize=22)
ax1.tick_params(axis='x', labelsize=14, rotation=0)
ax1.tick_params(axis='y', labelsize=14)
ax2.tick_params(axis='y', labelsize=14)

lines, labels = ax1.get_legend_handles_labels()
lines2, labels2 = ax2.get_legend_handles_labels()
ax1.legend(lines + lines2, labels + labels2, loc='upper right', fontsize=14)

plt.tight_layout()
plt.show()

first_attempts = df_all[df_all["attempt"] == "attempt_1"]
first_attempt_completion = first_attempts[first_attempts["completed"]].groupby("level_name")["player_id"].nunique()
total_players = df_all.groupby("level_name")["player_id"].nunique()
first_attempt_failures = total_players - first_attempt_completion

completion_df = pd.DataFrame({
    "Completed on First Attempt": first_attempt_completion,
    "Failed First Attempt": first_attempt_failures
}).fillna(0).reindex(level_order)

completion_df_percent = completion_df.div(completion_df.sum(axis=1), axis=0) * 100

ax = completion_df_percent.plot(kind="bar", stacked=True, figsize=(14, 8), color=sns.color_palette("Set2"), edgecolor="black")

plt.ylabel("Percentage of Players", fontsize=16)
plt.xlabel("Level", fontsize=16)
plt.title("First Attempt Completion Rate per Level (%)", fontsize=22)
plt.xticks(rotation=0, fontsize=14)
plt.yticks(fontsize=14)
plt.legend(title="Attempt Result", fontsize=14, title_fontsize=16, loc='upper right')

for i, (index, row) in enumerate(completion_df_percent.iterrows()):
    bottom = 0
    for col in completion_df_percent.columns:
        height = row[col]
        if height > 0:
            ax.text(i, bottom + height / 2, f"{height:.1f}%", ha='center', va='center', fontsize=10)
        bottom += height

plt.tight_layout()
plt.show()

fig, axes = plt.subplots(2, 2, figsize=(16, 12))
axes = axes.flatten()
max_retry = df_all[df_all['completed']]["retries"].max()
bins = list(range(0, int(max_retry) + 2))

for idx, level_name in enumerate(level_order):
    level_data = df_all[(df_all['completed']) & (df_all['level_name'] == level_name)]
    axes[idx].hist(level_data["retries"], bins=bins, color='skyblue', edgecolor='black', align='left', rwidth=0.8)
    axes[idx].set_title(f"Retry Count - {level_name}", fontsize=18)
    axes[idx].set_xlabel("Retries", fontsize=14)
    axes[idx].set_ylabel("Player Count", fontsize=14)
    axes[idx].tick_params(axis='x', labelsize=12)
    axes[idx].tick_params(axis='y', labelsize=12)
    axes[idx].set_xticks(bins)

plt.suptitle("Retry Count Distribution per Level", fontsize=22, y=1.02)
plt.tight_layout()
plt.show()

completed_only = df_all[df_all["completed"] & df_all["completionTime"].notna()]
plt.figure(figsize=(14, 8))
sns.boxplot(x="level_name", y="completionTime", data=completed_only, palette="deep", order=level_order)

plt.title("Completion Time Distribution per Level", fontsize=22)
plt.xlabel("Level", fontsize=16)
plt.ylabel("Completion Time (seconds)", fontsize=16)
plt.xticks(rotation=0, fontsize=14)
plt.yticks(fontsize=14)
plt.tight_layout()
plt.show()

expected_times = {
    "Basic Tutorial": 60,
    "Ally Tutorial": 80,
    "First Level": 180,
    "Second Level": 240
}
avg_times = completed_only.groupby("level_name")["completionTime"].mean().to_dict()
df_all["time_range"] = df_all["completionTime"].apply(label_time_bins)

time_range_pivot = (
    df_all[df_all["completed"] == True]
    .groupby(["level_name", "time_range"]).size()
    .unstack(fill_value=0)
    .reindex(level_order)
    .reindex(columns=time_range_order)
)

ax = time_range_pivot.plot(kind='bar', stacked=True, figsize=(16, 10), color=sns.color_palette("Set2"), edgecolor='black')

plt.title("Completion Time Buckets per Level (Stacked)", fontsize=22)
plt.xlabel("Level", fontsize=16)
plt.ylabel("Number of Players", fontsize=16)
plt.xticks(rotation=0, fontsize=14)
plt.yticks(fontsize=14)
plt.legend(title="Time Range", fontsize=14, title_fontsize=16)

for i, level in enumerate(level_order):
    avg = avg_times.get(level, 0)
    exp = expected_times[level]
    total = time_range_pivot.loc[level].sum()
    ax.text(i, total + 2, f"Avg: {avg:.1f}s", ha='center', fontsize=10, color='black')
    ax.text(i, total + 4, f"Exp: {exp}s", ha='center', fontsize=10, color='gray')

plt.tight_layout()
plt.show()

heatmap_data = df_all[df_all['completed']].pivot_table(index='retries', columns='level_name', values='completionTime', aggfunc='mean')

plt.figure(figsize=(14, 10))
sns.heatmap(heatmap_data, cmap='coolwarm', annot=True, fmt=".1f", linewidths=.5, cbar_kws={'label': 'Avg Completion Time (s)'})

plt.title("Average Completion Time by Retries per Level", fontsize=22)
plt.xlabel("Level", fontsize=16)
plt.ylabel("Retries", fontsize=16)
plt.xticks(fontsize=14)
plt.yticks(fontsize=14)
plt.tight_layout()
plt.show()

completed = df_all[df_all["completed"] & df_all["completionTime"].notna()]
completed["expected"] = completed["level_name"].map(expected_times).astype(float)
outliers = completed[completed["completionTime"] > 2 * completed["expected"]]

if not outliers.empty:
    table_data = outliers[["player_id", "level_name", "completionTime", "expected"]].copy()
    table_data["completionTime"] = table_data["completionTime"].round(2)
    table_data["expected"] = table_data["expected"].round(2)
    table_data.columns = ["Player ID", "Level", "Completion Time", "Expected Time"]

    fig, ax = plt.subplots(figsize=(14, len(table_data) * 0.3 + 1))
    ax.axis('off')
    ax.axis('tight')

    table = ax.table(
        cellText=table_data.values,
        colLabels=table_data.columns,
        cellLoc='center',
        loc='center'
    )
    table.auto_set_font_size(False)
    table.set_fontsize(10)
    table.scale(1, 1.4)

    plt.title("Outliers: Players Taking >2x Expected Time", fontsize=18, pad=20)
    plt.tight_layout()
    plt.show()