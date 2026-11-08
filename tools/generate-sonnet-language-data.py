#!/usr/bin/env python3
"""Generate Sonnet's checked-in language resources; no third-party Python packages.

Ordinary builds consume the generated files offline. To regenerate during a build,
set GenerateSonnetLanguageData=true (Python 3 and network access are required).
Use --source-root with pinned jieba/ and tinysegmenter/ checkouts for offline work.
Only literal data is read from upstream Python: source code is never executed.
"""
import argparse
import ast
import hashlib
import math
import pathlib
import struct
import urllib.request

REVISIONS = {
    'jieba': ('fxsjy/jieba', '67fa2e36e72f69d9134b8a1037b83fbb070b9775'),
    'tinysegmenter': ('SamuraiT/tinysegmenter', '9a4d0e64e09d0595f9ac7ea2ebbecd240579a1cf'),
}
# Pinned SHA-256 values validate upstream inputs before generation.
SOURCES = {
    'jieba/extra_dict/dict.txt.big': 'b16011275c42955ccd81fc1adecc93a59dbb7926af69d93fc95d4943d40f6aad',
    'jieba/jieba/finalseg/prob_start.py': '14c5706ced5cd3b42eb4873d4b88f7f52a7bdf80fbd767bc4423d361e20c5330',
    'jieba/jieba/finalseg/prob_trans.py': '54dfbc252ed71480d4f0cdfdf516ecfbe44efd0f6c3c64b158e7039f2906c91b',
    'jieba/jieba/finalseg/prob_emit.py': '27d46b1c9efe4dd148fde8be042a21be40e3562d0c7f1273f9de7abae12ebb8d',
    'jieba/LICENSE': '18ba0984839f85853b29fadaf992f7dba8fd0ca0fbeae34de2b8735222dc7a37',
    'tinysegmenter/tinysegmenter/tinysegmenter.py': '0ebf026d6c1692fda66d2015f0bd3ab6f254000d4ba7ef117a9d0ac6ee888e61',
    'tinysegmenter/LICENSE.md': 'efda736cd061673847d74e823f282e65a448bfe65a14260b52a81d26ae50d5e2',
}
ROOT = pathlib.Path(__file__).resolve().parents[1]
OUTPUT = ROOT / 'src/Libraries/AvaloniaSilkEffects/Sonnet/LanguageData'


def varint(value):
    result = bytearray()
    while value >= 128:
        result.append((value & 127) | 128)
        value >>= 7
    result.append(value)
    return result


def literals(source):
    result = {}
    for node in ast.parse(source).body:
        if isinstance(node, ast.Assign) and isinstance(node.targets[0], ast.Name):
            try:
                result[node.targets[0].id] = ast.literal_eval(node.value)
            except (ValueError, TypeError):
                pass
    return result


def pos_tag(tag):
    # Shared wire values: Other, Noun, Verb, Adjective, Adverb, Pronoun, Function.
    return {'n': 1, 'v': 2, 'a': 3, 'd': 4, 'r': 5,
            'u': 6, 'p': 6, 'c': 6, 'y': 6}.get(tag[:1], 0)


def dictionary(source):
    words = {}
    for line in source.decode('utf-8').splitlines():
        word, frequency, tag = line.split()
        words[word.encode('utf-8')] = (int(frequency), pos_tag(tag))
    ordered = sorted(words)
    total = sum(freq for freq, _ in words.values())
    max_utf16 = max(len(word.decode('utf-8').encode('utf-16-le')) // 2 for word in ordered)
    payload = bytearray()
    offsets = []
    previous = b''
    for index, word in enumerate(ordered):
        if index % 32 == 0:
            offsets.append(len(payload))
            previous = b''
        prefix = 0
        while prefix < min(len(previous), len(word)) and previous[prefix] == word[prefix]:
            prefix += 1
        suffix = word[prefix:]
        frequency, tag = words[word]
        payload += varint(prefix) + varint(len(suffix)) + suffix
        payload += struct.pack('<fB', math.log(max(1, frequency)) - math.log(total), tag)
        previous = word
    header_size = 28 + 4 * len(offsets)
    header = struct.pack('<4siiiid', b'SNL1', len(words), max_utf16,
                         max(map(len, ordered)), len(offsets), math.log(total))
    header += struct.pack('<' + 'i' * len(offsets), *(header_size + offset for offset in offsets))
    return header + payload, len(words)


def hmm(start, transitions, emissions):
    states = 'BMES'
    minimum = -3.14e100
    output = bytearray(b'SNH1')
    output += struct.pack('<4d', *(start.get(s, minimum) for s in states))
    output += struct.pack('<16d', *(transitions.get(s, {}).get(t, minimum) for s in states for t in states))
    for state in states:
        entries = sorted((ord(char), probability) for char, probability in emissions[state].items())
        output += struct.pack('<i', len(entries))
        for scalar, probability in entries:
            output += struct.pack('<id', scalar, probability)
    return output


def japanese(model):
    tables = sorted((name[1:], value) for name, value in model.items()
                    if name.startswith('_') and isinstance(value, dict) and value)
    output = bytearray(struct.pack('<4sii', b'SNJ1', model['_BIAS'], len(tables)))
    for name, entries in tables:
        encoded_name = name.encode('ascii')
        output += varint(len(encoded_name)) + encoded_name + struct.pack('<i', len(entries))
        for key, value in sorted(entries.items()):
            encoded = key.encode('utf-8')
            output += varint(len(encoded)) + encoded + struct.pack('<i', value)
    return output


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source-root', type=pathlib.Path)
    args = parser.parse_args()

    def read(project, path):
        repository, revision = REVISIONS[project]
        url = f'https://raw.githubusercontent.com/{repository}/{revision}/{path}'
        if args.source_root:
            data = (args.source_root / project / path).read_bytes()
        else:
            with urllib.request.urlopen(url, timeout=60) as response:
                data = response.read()
        digest = hashlib.sha256(data).hexdigest()
        expected = SOURCES[f'{project}/{path}']
        if digest != expected:
            raise ValueError(f'Source checksum mismatch: {project}/{path}')
        return data

    lexicon, count = dictionary(read('jieba', 'extra_dict/dict.txt.big'))
    model = [literals(read('jieba', f'jieba/finalseg/prob_{name}.py').decode('utf-8'))['P']
             for name in ('start', 'trans', 'emit')]
    resources = {'Chinese.bin': lexicon, 'ChineseHmm.bin': hmm(*model),
                 'Japanese.bin': japanese(literals(read('tinysegmenter', 'tinysegmenter/tinysegmenter.py').decode('utf-8')))}
    size = sum(map(len, resources.values()))
    if size > 8 * 1024 * 1024:
        raise ValueError(f'Language data exceeds 8 MiB: {size} bytes; no entries were pruned.')
    licenses = {'Jieba.LICENSE.txt': read('jieba', 'LICENSE'),
                'TinySegmenter.LICENSE.txt': read('tinysegmenter', 'LICENSE.md')}
    OUTPUT.mkdir(parents=True, exist_ok=True)
    for name, data in {**resources, **licenses}.items():
        if name.endswith('.txt'):
            data = data.rstrip(b'\r\n') + b'\n'
        (OUTPUT / name).write_bytes(data)
    print(f'Sonnet language data: {count} dictionary entries, {size:,} bytes ({size / 1048576:.2f} MiB)')


if __name__ == '__main__':
    main()
