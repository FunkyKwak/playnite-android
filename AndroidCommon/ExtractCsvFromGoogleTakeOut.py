from datetime import datetime
import json
import re
import urllib.parse
import urllib.request
import zipfile


def get_real_package_name(game_title: str) -> str:
    """Cherche le jeu directement sur le Google Play Store pour récupérer son package name officiel."""
    # Nettoyage préalable du nom (ex: "Hitman_ Sniper" -> "Hitman Sniper")
    clean_title = game_title.replace('_', ' ').strip()
    query = urllib.parse.quote(clean_title)
    url = f'https://play.google.com/store/search?q={query}&c=apps&hl=fr'

    req = urllib.request.Request(
        url,
        headers={
            'User-Agent': (
                'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
                ' (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36'
            )
        },
    )

    try:
        with urllib.request.urlopen(req, timeout=5) as resp:
            html = resp.read().decode('utf-8', errors='ignore')
            # Recherche du paramètre ?id=com.xxx.yyy dans la page de recherche
            matches = re.findall(
                r'/store/apps/details\?id=([a-zA-Z0-9_.]+)', html
            )
            if matches:
                # Retourne le premier package name trouvé
                return matches[0]
    except Exception:
        pass

    # Solution de repli si le jeu n'est plus sur le store : génération d'un slug
    slug = re.sub(r'[^a-zA-Z0-9]', '', clean_title).lower()
    return f'com.unknown.{slug}'


def extract_timestamps(obj):
    """Parcourt récursivement les objets JSON pour extraire les horodatages."""
    ts_list = []
    if isinstance(obj, dict):
        for k, v in obj.items():
            if isinstance(v, (int, float)):
                if any(
                    t in k.lower() for t in ['millis', 'timestamp', 'time', 'date']
                ):
                    if 946684800000 < v < 1800000000000:
                        ts_list.append(v / 1000.0)
                    elif 946684800 < v < 1800000000:
                        ts_list.append(v)
            elif isinstance(v, str):
                if v.isdigit():
                    val = float(v)
                    if 946684800000 < val < 1800000000000:
                        ts_list.append(val / 1000.0)
                    elif 946684800 < val < 1800000000:
                        ts_list.append(val)
                else:
                    try:
                        dt = datetime.fromisoformat(v.replace('Z', '+00:00'))
                        ts_list.append(dt.timestamp())
                    except Exception:
                        pass
            elif isinstance(v, (dict, list)):
                ts_list.extend(extract_timestamps(v))
    elif isinstance(obj, list):
        for item in obj:
            ts_list.extend(extract_timestamps(item))
    return ts_list


def parse_takeout_to_csv(
    zip_path: str, output_csv_path: str = 'jeux_takeout.csv'
):
    games = {}

    print('Lecture de l’archive Takeout...')
    with zipfile.ZipFile(zip_path, 'r') as z:
        for f in z.namelist():
            if 'Services de jeux Google' in f and '/Jeux/' in f:
                parts = re.split(
                    r'Services de jeux Google[\s\xa0]Play/Jeux/', f
                )
                if len(parts) > 1 and parts[1]:
                    path_parts = parts[1].split('/')
                    game_name = path_parts[0]
                    if not game_name:
                        continue

                    if game_name not in games:
                        games[game_name] = {'max_ts': None}

                    if f.endswith('.json'):
                        try:
                            data = json.loads(
                                z.read(f).decode('utf-8', errors='ignore')
                            )
                            timestamps = extract_timestamps(data)
                            for ts in timestamps:
                                if (
                                    games[game_name]['max_ts'] is None
                                    or ts > games[game_name]['max_ts']
                                ):
                                    games[game_name]['max_ts'] = ts
                        except Exception:
                            pass

    print(
        f'{len(games)} jeux identifiés. Récupération des vrais package names'
        ' sur le Play Store...'
    )

    rows = []
    for idx, (gname, info) in enumerate(games.items(), 1):
        clean_name = gname.replace('_', ': ')

        # Récupération du package name officiel
        pkg_name = get_real_package_name(gname)

        if info['max_ts']:
            date_str = datetime.fromtimestamp(info['max_ts']).strftime(
                '%Y-%m-%d'
            )
        else:
            date_str = 'N/A'

        rows.append((pkg_name, clean_name, date_str))
        print(f'[{idx}/{len(games)}] {clean_name} -> {pkg_name}')

    # Tri par date de dernière utilisation (du plus récent au plus ancien)
    rows.sort(key=lambda x: x[2], reverse=True)

    with open(output_csv_path, 'w', encoding='utf-8') as out:
        out.write('package,name,last_time_used\n')
        for pkg, name, dt in rows:
            out.write(f'{pkg},{name},{dt}\n')

    print(f'\nTerminé ! Fichier généré : "{output_csv_path}".')


# Remplace par le nom exact de ton fichier .zip
zip_filename = "C:\\Users\\Yann\\Downloads\\takeout-20260930T130711Z-1-001.zip"
parse_takeout_to_csv(zip_filename)