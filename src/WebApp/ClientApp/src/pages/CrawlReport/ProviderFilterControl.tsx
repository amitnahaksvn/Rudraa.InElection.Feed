import { useMemo, useState } from 'react';
import Autocomplete from '@mui/material/Autocomplete';
import TextField from '@mui/material/TextField';
import Checkbox from '@mui/material/Checkbox';
import Chip from '@mui/material/Chip';
import Stack from '@mui/material/Stack';
import FormControlLabel from '@mui/material/FormControlLabel';
import Switch from '@mui/material/Switch';
import CheckBoxOutlineBlankIcon from '@mui/icons-material/CheckBoxOutlineBlank';
import CheckBoxIcon from '@mui/icons-material/CheckBox';
import { ProviderLogo } from '../ProviderManagement/ProviderLogo';
import { useProviderCatalog } from './useProviderCatalog';

export interface ProviderFilterControlProps {
  pipeline: 'Rss' | 'Api';
  selected: string[];
  onChange: (providers: string[]) => void;
}

// Disabled providers vastly outnumber enabled ones in this app's catalog (most of the 300+
// configured providers are disabled for one language/country reason or another - see CLAUDE.md),
// so the selector hides them by default rather than burying the enabled ones a reader actually
// wants in a huge dropdown; "Show disabled providers" reveals the rest on demand. A provider
// already selected stays visible regardless of the toggle, so switching the toggle off never
// silently drops a selection out from under the reader.
export function ProviderFilterControl({ pipeline, selected, onChange }: ProviderFilterControlProps) {
  const { providers, isLoading } = useProviderCatalog(pipeline);
  const [showDisabled, setShowDisabled] = useState(false);

  const options = useMemo(
    () => providers.filter((p) => p.enabled || showDisabled || selected.includes(p.name)),
    [providers, showDisabled, selected],
  );

  const selectedEntries = useMemo(
    () => providers.filter((p) => selected.includes(p.name)),
    [providers, selected],
  );

  const hiddenDisabledCount = providers.length - options.length;

  return (
    <Stack direction="row" alignItems="center" gap={1.5} flexWrap="wrap">
      <Autocomplete
        multiple
        size="small"
        loading={isLoading}
        sx={{ minWidth: 320, flex: '1 1 320px' }}
        options={options}
        disableCloseOnSelect
        getOptionLabel={(option) => option.name}
        isOptionEqualToValue={(option, value) => option.name === value.name}
        value={selectedEntries}
        onChange={(_, next) => onChange(next.map((entry) => entry.name))}
        renderOption={(props, option, { selected: isSelected }) => {
          const { key, ...rest } = props;
          return (
            <li key={key} {...rest}>
              <Checkbox
                icon={<CheckBoxOutlineBlankIcon fontSize="small" />}
                checkedIcon={<CheckBoxIcon fontSize="small" />}
                checked={isSelected}
                size="small"
                sx={{ mr: 1 }}
              />
              <ProviderLogo name={option.name} domain={option.domain} size={22} />
              <Stack direction="row" alignItems="center" gap={1} sx={{ ml: 1, minWidth: 0 }}>
                <span>{option.name}</span>
                {!option.enabled && <Chip label="Disabled" size="small" variant="outlined" />}
              </Stack>
            </li>
          );
        }}
        renderTags={(value, getTagProps) =>
          value.map((option, index) => {
            const { key, ...tagProps } = getTagProps({ index });
            return (
              <Chip
                key={key}
                {...tagProps}
                size="small"
                avatar={<ProviderLogo name={option.name} domain={option.domain} size={20} />}
                label={option.name}
              />
            );
          })
        }
        renderInput={(params) => (
          <TextField
            {...params}
            label="Filter by provider"
            placeholder={selected.length ? undefined : 'All enabled providers'}
          />
        )}
      />
      <FormControlLabel
        control={<Switch size="small" checked={showDisabled} onChange={(e) => setShowDisabled(e.target.checked)} />}
        label={`Show disabled providers${hiddenDisabledCount > 0 && !showDisabled ? ` (${hiddenDisabledCount})` : ''}`}
        sx={{ whiteSpace: 'nowrap' }}
      />
    </Stack>
  );
}
