import os
import glob
import pandas as pd
import json

ROOT = r"D:\BLverse-Life_Simulation_Game\GridBrain"
EXTS = ("**/*.csv", "**/*.tsv", "**/*.jsonl", "**/*.json", "**/*.parquet", "**/*.xlsx")

def head_csv(p, sep=','):
    try:
        df = pd.read_csv(p, nrows=5, sep=sep, encoding='utf-8', engine='python')
        return list(df.columns), df.dtypes.to_dict(), df.head().to_dict(orient='list')
    except Exception as e:
        # fallback to first line
        try:
            with open(p, 'r', encoding='utf-8', errors='ignore') as f:
                hdr = f.readline().strip().split(sep)
            return hdr, {}, {}
        except:
            return [], {}, {}

def head_jsonl(p):
    try:
        with open(p, 'r', encoding='utf-8', errors='ignore') as f:
            first = f.readline()
            obj = json.loads(first)
            if isinstance(obj, dict):
                return list(obj.keys()), {}, {k: obj.get(k) for k in list(obj.keys())[:5]}
    except:
        pass
    return [], {}, {}

def head_parquet(p):
    try:
        df = pd.read_parquet(p, engine='auto')
        return list(df.columns), df.dtypes.to_dict(), df.head().to_dict(orient='list')
    except:
        return [], {}, {}

def head_excel(p):
    try:
        df = pd.read_excel(p, nrows=5)
        return list(df.columns), df.dtypes.to_dict(), df.head().to_dict(orient='list')
    except:
        return [], {}, {}

handlers = {
    '.csv': lambda p: head_csv(p, sep=','),
    '.tsv': lambda p: head_csv(p, sep='\t'),
    '.jsonl': head_jsonl,
    '.json': lambda p: (lambda df: (list(df.columns), df.dtypes.to_dict(), df.head().to_dict(orient='list'))) (pd.read_json(p, lines=False)) if os.path.getsize(p)>0 else ([],{},{}),
    '.parquet': head_parquet,
    '.xlsx': head_excel,
}

for pattern in EXTS:
    for path in glob.glob(os.path.join(ROOT, pattern), recursive=True):
        ext = os.path.splitext(path)[1].lower()
        cols, dtypes, sample = ([], {}, {})
        if ext in handlers:
            try:
                cols, dtypes, sample = handlers[ext](path)
            except Exception:
                cols, dtypes, sample = [],{},{}
        print(f"{path}")
        if cols:
            print("  columns:", ", ".join(map(str, cols)))
            if dtypes:
                dstr = ", ".join(f"{k}:{v}" for k,v in list(dtypes.items())[:10])
                print("  dtypes (sample):", dstr)
            if sample:
                # print up to 3 keys with sample values
                keys = list(sample.keys())[:3]
                for k in keys:
                    vals = sample[k]
                    print(f"   sample {k} -> {vals[:3] if isinstance(vals,list) else vals}")
        else:
            print("  (could not read columns or empty file)")
        print("-"*60)