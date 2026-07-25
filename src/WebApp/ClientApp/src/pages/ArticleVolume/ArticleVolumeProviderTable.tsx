import { useMemo, useState } from 'react';
import Box from '@mui/material/Box';
import Stack from '@mui/material/Stack';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import TableSortLabel from '@mui/material/TableSortLabel';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import Chip from '@mui/material/Chip';
import InputAdornment from '@mui/material/InputAdornment';
import SearchIcon from '@mui/icons-material/Search';
import type { ArticleVolumeProviderRow } from '../../api/crawlTypes';
import { formatFullNumber } from '../../utils/formatNumber';
import { useChartColors } from '../CrawlReport/useChartColorMode';
import { ProviderLogo } from '../ProviderManagement/ProviderLogo';
import type { ProviderCatalogEntry } from '../CrawlReport/useProviderCatalog';

type SortKey = 'provider' | 'count';

const COLUMNS: { key: SortKey; label: string; align?: 'right' }[] = [
  { key: 'provider', label: 'Provider' },
  { key: 'count', label: 'Articles', align: 'right' },
];

// A bar proportional to the busiest provider in the current table, not a percent-of-total meter
// (SuccessRateMeter's "ratio against a fixed 100% limit" shape doesn't fit here - there's no
// natural ceiling for an article count, only "more or less than the others on screen").
function VolumeBar({ count, max }: { count: number; max: number }) {
  const colors = useChartColors();
  const pct = max <= 0 ? 0 : Math.min(100, (count / max) * 100);

  return (
    <Box sx={{ position: 'relative', width: 90, height: 6, borderRadius: 3, bgcolor: colors.meterTrack, overflow: 'hidden' }}>
      <Box sx={{ position: 'absolute', inset: 0, width: `${pct}%`, bgcolor: colors.seriesNew, borderRadius: 3 }} />
    </Box>
  );
}

function compareValues(a: ArticleVolumeProviderRow, b: ArticleVolumeProviderRow, key: SortKey): number {
  if (key === 'count') return a.count - b.count;
  return a.provider.localeCompare(b.provider);
}

export interface ArticleVolumeProviderTableProps {
  rows: ArticleVolumeProviderRow[];
  catalog: ProviderCatalogEntry[];
}

export function ArticleVolumeProviderTable({ rows, catalog }: ArticleVolumeProviderTableProps) {
  const [search, setSearch] = useState('');
  const [sortKey, setSortKey] = useState<SortKey>('count');
  const [sortDir, setSortDir] = useState<'asc' | 'desc'>('desc');

  const catalogByName = useMemo(() => new Map(catalog.map((c) => [c.name, c])), [catalog]);
  const maxCount = useMemo(() => rows.reduce((max, r) => Math.max(max, r.count), 0), [rows]);

  const filteredSorted = useMemo(() => {
    const term = search.trim().toLowerCase();
    const filtered = term ? rows.filter((r) => r.provider.toLowerCase().includes(term)) : rows;
    return [...filtered].sort((a, b) => compareValues(a, b, sortKey) * (sortDir === 'asc' ? 1 : -1));
  }, [rows, search, sortKey, sortDir]);

  const handleSort = (key: SortKey) => {
    if (key === sortKey) {
      setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'));
    } else {
      setSortKey(key);
      setSortDir(key === 'count' ? 'desc' : 'asc');
    }
  };

  return (
    <Box>
      <TextField
        size="small"
        placeholder="Search provider..."
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        sx={{ mb: 1, width: 280 }}
        slotProps={{ input: { startAdornment: <InputAdornment position="start"><SearchIcon fontSize="small" /></InputAdornment> } }}
      />
      <TableContainer sx={{ maxHeight: 520 }}>
        <Table size="small" stickyHeader>
          <TableHead>
            <TableRow>
              {COLUMNS.map((col) => (
                <TableCell key={col.key} align={col.align} sortDirection={sortKey === col.key ? sortDir : false}>
                  <TableSortLabel active={sortKey === col.key} direction={sortDir} onClick={() => handleSort(col.key)}>
                    {col.label}
                  </TableSortLabel>
                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredSorted.map((row) => {
              const entry = catalogByName.get(row.provider);
              return (
                <TableRow key={row.provider} hover>
                  <TableCell>
                    <Stack direction="row" alignItems="center" gap={1}>
                      <ProviderLogo name={row.provider} domain={entry?.domain} size={28} />
                      <Typography variant="body2" fontWeight={600}>
                        {row.provider}
                      </Typography>
                      {entry && !entry.enabled && <Chip label="Disabled" size="small" variant="outlined" />}
                    </Stack>
                  </TableCell>
                  <TableCell align="right">
                    <Stack direction="row" alignItems="center" justifyContent="flex-end" gap={1.5}>
                      <VolumeBar count={row.count} max={maxCount} />
                      <Typography variant="body2" sx={{ fontVariantNumeric: 'tabular-nums', minWidth: 48, textAlign: 'right' }}>
                        {formatFullNumber(row.count)}
                      </Typography>
                    </Stack>
                  </TableCell>
                </TableRow>
              );
            })}
            {filteredSorted.length === 0 && (
              <TableRow>
                <TableCell colSpan={COLUMNS.length} align="center">
                  <Typography variant="body2" color="text.secondary" sx={{ py: 3 }}>
                    No providers match your search.
                  </Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}
