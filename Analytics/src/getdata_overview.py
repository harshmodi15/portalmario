import requests
import csv
import os

FIREBASE_BASE_URL = "https://portalmario-cs526-default-rtdb.firebaseio.com"
EXPORT_DIR = "Analytics/Beta_Data/Beta_Overview"

SECTIONS = [
    ("level_-1", "level_-1"),
    ("level_0", "level_0"),
    ("level_1", "level_1"),
    ("level_2", "level_2"),
]

os.makedirs(EXPORT_DIR, exist_ok=True)

def fetch_test_completion_data():
    url = f"{FIREBASE_BASE_URL}/testCompletion.json"
    response = requests.get(url)
    if response.status_code == 200:
        return response.json()
    else:
        print(f"Error {response.status_code}: {response.text}")
        return None

def export_section_attempts_to_csv(data):
    for section_key, filename in SECTIONS:
        section_results = []

        for player_id, player_data in data.items():
            level_data = player_data.get(section_key)
            if not level_data:
                continue

            for attempt_name, attempt_info in level_data.items():
                row = {
                    "player_id": player_id,
                    "level": section_key,
                    "attempt": attempt_name,
                    "completed": attempt_info.get("completed", False),
                    "completionTime": attempt_info.get("completionTime", "N/A"),
                    "deaths": attempt_info.get("deaths", 0),
                    "retries": attempt_info.get("retries", 0),
                }
                section_results.append(row)

        filepath = os.path.join(EXPORT_DIR, f"{filename}.csv")
        with open(filepath, mode='w', newline='') as f:
            writer = csv.DictWriter(f, fieldnames=section_results[0].keys())
            writer.writeheader()
            writer.writerows(section_results)

        print(f"Exported {filename}.csv with {len(section_results)} attempts")

if __name__ == "__main__":
    data = fetch_test_completion_data()
    if data:
        export_section_attempts_to_csv(data)