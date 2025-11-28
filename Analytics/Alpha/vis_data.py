import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt

data_path = "Analytics/GameAnalytics.xlsx"
l0 = pd.read_excel(data_path, sheet_name="Level 0")
l1 = pd.read_excel(data_path, sheet_name="Level 1")

concat_data = pd.concat([l0, l1], ignore_index=True)

avg_completion_rate = concat_data.groupby("Level")["Completed"].mean().reset_index()
avg_completion_rate["Completion Rate"] = avg_completion_rate["Completed"] * 100

ax = sns.barplot(x="Level", y="Completion Rate", data=avg_completion_rate)

for i, value in enumerate(avg_completion_rate["Completion Rate"]):
    ax.text(i, value + 2, f"{value:.1f}%", ha="center", fontsize=12, fontweight="bold")

plt.title("Average completion rate per level")
plt.show()

# Avg deaths per level
avg_deaths = concat_data.groupby("Level")["Deaths"].mean().reset_index()
sns.barplot(x="Level", y="Deaths", data=avg_deaths)
plt.title("Average Deaths per Level")
plt.show()

# Death histogram
sns.histplot(concat_data, x='Deaths', hue='Level', bins=10, multiple='dodge', shrink=0.8)#, kde=True)
plt.title('Deaths Distribution by Level')
plt.show()

# Avg retry per level
avg_retry = concat_data.groupby("Level")["Retries"].mean().reset_index()
sns.barplot(x="Level", y="Retries", data=avg_retry)
plt.title("Average Retries per Level")
plt.show()

# Retry histogram
sns.histplot(concat_data, x='Retries', hue='Level', bins=10, multiple='dodge', shrink=0.8)#, kde=True)
plt.title('Retry Distribution by Level')
plt.show()



# Completion Time
sns.histplot(concat_data, x="Completion Time", hue="Level", bins=20, multiple='dodge', shrink=0.8)
plt.title("Competion Time distribution by Level")
plt.show()

