import firebase_admin
from firebase_admin import credentials, db
import pandas as pd

cred = credentials.Certificate("Analytics/portalmario-cs526-key.json")
firebase_admin.initialize_app(cred, {
    'databaseURL': 'https://portalmario-cs526-default-rtdb.firebaseio.com/'
})

ref = db.reference("levelCompletion")
data = ref.get()

rows = []
if data:
    for player_id, player_data in data.items():
        for level, stats in player_data.items():
            rows.append({
                "Player ID": player_id,
                "Level": level,
                "Completion Time": stats.get("completionTime", "N/A"),
                "Deaths": stats.get("deaths", 0),
                "Retries": stats.get("retries", 0),
                "Completed": stats.get("completed", False)
            })

df = pd.DataFrame(rows)

with pd.ExcelWriter("Analytics/Alpha/Alpha_Details.xlsx") as writer:
    df[df["Level"] == "level_0"].to_excel(writer, sheet_name="Level 0", index=False)
    df[df["Level"] == "level_1"].to_excel(writer, sheet_name="Level 1", index=False)

print("Data saved to Analytics/Alpha/Alpha_Details.xlsx")