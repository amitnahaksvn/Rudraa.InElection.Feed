import { useState, useEffect, useMemo } from 'react';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import InputAdornment from '@mui/material/InputAdornment';
import Button from '@mui/material/Button';
import Badge from '@mui/material/Badge';
import Chip from '@mui/material/Chip';
import Collapse from '@mui/material/Collapse';
import SearchIcon from '@mui/icons-material/Search';
import FilterListIcon from '@mui/icons-material/FilterList';
import type { ErrorLogFilters } from '../../api/types';

interface FilterBarProps {
  filters: ErrorLogFilters;
  onChange: (filters: ErrorLogFilters) => void;
}

const ADVANCED_FIELDS = [
  { key: 'provider', label: 'Provider' },
  { key: 'country', label: 'Country' },
  { key: 'source', label: 'Source' },
] as const;

// Debounces free-text fields (search/provider/country/source) so every keystroke doesn't fire a
// new request. The status filter lives in StatusSidebar now (a discrete click, not typing), not
// here. Provider/Country/Source live behind a collapsible "Filters" panel rather than three
// always-visible text fields - on a phone-width list pane, three full-width inputs ate most of
// the visible list before a single result even showed; a badge on the toggle button plus
// removable chips for whichever ones are actually set keeps that same filtering power without
// permanently spending the vertical space.
export function FilterBar({ filters, onChange }: FilterBarProps) {
  const [draft, setDraft] = useState(filters);
  const [advancedOpen, setAdvancedOpen] = useState(false);

  useEffect(() => setDraft(filters), [filters]);

  useEffect(() => {
    const handle = setTimeout(() => {
      if (JSON.stringify(draft) !== JSON.stringify(filters)) {
        onChange(draft);
      }
    }, 350);
    return () => clearTimeout(handle);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [draft]);

  const activeAdvanced = useMemo(
    () => ADVANCED_FIELDS.filter((field) => draft[field.key].trim().length > 0),
    [draft],
  );

  const clearField = (key: (typeof ADVANCED_FIELDS)[number]['key']) => {
    const next = { ...draft, [key]: '' };
    setDraft(next);
    onChange(next);
  };

  return (
    <Stack gap={1} sx={{ p: 1.5 }}>
      <TextField
        size="small"
        placeholder="Search errors..."
        fullWidth
        value={draft.search}
        onChange={(e) => setDraft({ ...draft, search: e.target.value })}
        slotProps={{
          input: {
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon fontSize="small" />
              </InputAdornment>
            ),
          },
        }}
      />

      <Stack direction="row" alignItems="center" gap={1} flexWrap="wrap">
        <Badge badgeContent={activeAdvanced.length} color="primary">
          <Button
            size="small"
            variant={advancedOpen ? 'contained' : 'outlined'}
            startIcon={<FilterListIcon fontSize="small" />}
            onClick={() => setAdvancedOpen((v) => !v)}
          >
            Filters
          </Button>
        </Badge>

        {!advancedOpen &&
          activeAdvanced.map((field) => (
            <Chip
              key={field.key}
              size="small"
              label={`${field.label}: ${draft[field.key]}`}
              onDelete={() => clearField(field.key)}
            />
          ))}
      </Stack>

      <Collapse in={advancedOpen}>
        <Stack direction={{ xs: 'column', sm: 'row' }} gap={1} sx={{ pt: 0.5 }}>
          <TextField
            size="small"
            placeholder="Provider"
            fullWidth
            value={draft.provider}
            onChange={(e) => setDraft({ ...draft, provider: e.target.value })}
          />
          <TextField
            size="small"
            placeholder="Country"
            fullWidth
            value={draft.country}
            onChange={(e) => setDraft({ ...draft, country: e.target.value })}
          />
          <TextField
            size="small"
            placeholder="Source"
            fullWidth
            value={draft.source}
            onChange={(e) => setDraft({ ...draft, source: e.target.value })}
          />
        </Stack>
      </Collapse>
    </Stack>
  );
}
