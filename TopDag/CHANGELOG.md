## 2.1.0

### Added

- Changelog

### Changed

- Target framework .NET Standard 2.1 -> .NET Standard 2.0
	- Increased compatability with older versions of .NET - no code changes required

## 2.0.0

### Fixed

- IGraph no longer extends IEnumerable
	- Extending IEnumerable messed up serialization of graphs (e.g. to JSON)
