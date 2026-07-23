import { useState } from 'react';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import DialogActions from '@mui/material/DialogActions';
import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import TextField from '@mui/material/TextField';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Switch from '@mui/material/Switch';
import IconButton from '@mui/material/IconButton';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import Divider from '@mui/material/Divider';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import { useCountries } from './useCountries';
import { useDeleteCountry, useUpsertCountry } from './useCountryMutations';
import type { CrawlPipelineName } from '../../api/providerTypes';

export interface CountriesManagerDialogProps {
  pipeline: CrawlPipelineName;
  open: boolean;
  onClose: () => void;
}

/** Add/enable-toggle/delete for the country-level grouping - disabling a country skips every provider under it entirely, same as disabling a provider skips every feed under it. */
export function CountriesManagerDialog({ pipeline, open, onClose }: CountriesManagerDialogProps) {
  const { data: countries, isLoading } = useCountries(pipeline);
  const upsertCountry = useUpsertCountry(pipeline);
  const deleteCountry = useDeleteCountry(pipeline);
  const [newName, setNewName] = useState('');

  const handleAdd = () => {
    const name = newName.trim();
    if (!name) return;
    upsertCountry.mutate({ name, enabled: true }, { onSuccess: () => setNewName('') });
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>{pipeline} countries</DialogTitle>
      <DialogContent>
        <Stack direction="row" gap={1} sx={{ mb: 2, mt: 1 }}>
          <TextField
            size="small"
            label="Add country"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleAdd()}
            fullWidth
          />
          <Button variant="contained" startIcon={<AddIcon fontSize="small" />} disabled={!newName.trim() || upsertCountry.isPending} onClick={handleAdd}>
            Add
          </Button>
        </Stack>

        {isLoading && (
          <Stack alignItems="center" sx={{ py: 3 }}>
            <CircularProgress size={24} />
          </Stack>
        )}

        <List dense>
          {countries?.map((country, index) => (
            <Stack key={country.id}>
              {index > 0 && <Divider component="li" />}
              <ListItem
                secondaryAction={
                  <Stack direction="row" alignItems="center" gap={0.5}>
                    <Switch
                      size="small"
                      checked={country.enabled}
                      onChange={(e) => upsertCountry.mutate({ name: country.name, enabled: e.target.checked })}
                    />
                    <IconButton size="small" aria-label={`Delete ${country.name}`} onClick={() => deleteCountry.mutate(country.name)}>
                      <DeleteOutlineIcon fontSize="small" />
                    </IconButton>
                  </Stack>
                }
              >
                <ListItemText primary={country.name} />
              </ListItem>
            </Stack>
          ))}
        </List>

        {countries?.length === 0 && !isLoading && (
          <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 2 }}>
            No countries yet.
          </Typography>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
}
