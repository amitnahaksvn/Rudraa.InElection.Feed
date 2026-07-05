import { useState, useEffect } from 'react';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import InputAdornment from '@mui/material/InputAdornment';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import ToggleButton from '@mui/material/ToggleButton';
import SearchIcon from '@mui/icons-material/Search';
import Paper from '@mui/material/Paper';
import type { ErrorLogFilters, ResolvedFilter } from '../../api/types';

interface FilterBarProps {
  filters: ErrorLogFilters;
  onChange: (filters: ErrorLogFilters) => void;
}

// Debounces free-text fields (search/provider/country/source) so every keystroke doesn't fire a
// new request - the status toggle applies immediately since it's a discrete click, not typing.
export function FilterBar({ filters, onChange }: FilterBarProps) {
  const [draft, setDraft] = useState(filters);

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

  return (
    <Paper variant="outlined" sx={{ p: 1.5, mb: 2 }}>
      <Stack gap={1.5}>
        <ToggleButtonGroup
          size="small"
          exclusive
          value={filters.status}
          onChange={(_, value: ResolvedFilter | null) => value && onChange({ ...filters, status: value })}
        >
          <ToggleButton value="unresolved">Unresolved</ToggleButton>
          <ToggleButton value="resolved">Resolved</ToggleButton>
          <ToggleButton value="all">All</ToggleButton>
        </ToggleButtonGroup>

        <Stack direction={{ xs: 'column', sm: 'row' }} gap={1.5}>
          <TextField
            size="small"
            placeholder="Search message, exception type..."
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
      </Stack>
    </Paper>
  );
}
