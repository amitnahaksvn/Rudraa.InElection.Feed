import { useState, type SyntheticEvent } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import Stack from '@mui/material/Stack';
import Switch from '@mui/material/Switch';
import TextField from '@mui/material/TextField';
import IconButton from '@mui/material/IconButton';
import CircularProgress from '@mui/material/CircularProgress';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import SaveIcon from '@mui/icons-material/Save';
import { updateProviderSchedule, type UpdateProviderScheduleFields } from '../../api/providers';
import type { CrawlPipelineName } from '../../api/providerTypes';

export interface ScheduleEditorProps {
  pipeline: CrawlPipelineName;
  provider: string;
  country: string;
  enabled: boolean;
  cron: string;
  timeZone: string;
  // Everything else UpdateProviderScheduleCommand needs to avoid resetting fields this editor
  // doesn't itself expose - passed straight through unchanged on every save.
  otherFields: Omit<UpdateProviderScheduleFields, 'pipeline' | 'provider' | 'country' | 'enabled' | 'cron' | 'timeZone'>;
}

/**
 * Live enable/disable + cron/timezone editor for one provider's schedule - persists to
 * ProviderSchedule (survives restarts) and updates the Hangfire recurring job immediately (see
 * UpdateProviderScheduleCommand). Rendered with `key={cron+enabled+timeZone}` by its parent card
 * so a server-side change (e.g. from another tab) resets this component's local draft state
 * instead of silently going stale.
 */
export function ScheduleEditor({ pipeline, provider, country, enabled, cron, timeZone, otherFields }: ScheduleEditorProps) {
  const queryClient = useQueryClient();
  const listQueryKey = [pipeline === 'Api' ? 'apiProviders' : 'rssProviders'];

  const [cronDraft, setCronDraft] = useState(cron);
  const [timeZoneDraft, setTimeZoneDraft] = useState(timeZone);

  const mutation = useMutation({
    mutationFn: (vars: { enabled: boolean; cron: string; timeZone: string }) =>
      updateProviderSchedule({ pipeline, provider, country, ...vars, ...otherFields }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: listQueryKey }),
  });

  const dirty = cronDraft !== cron || timeZoneDraft !== timeZone;

  const stop = (e: SyntheticEvent) => e.stopPropagation();

  return (
    <Stack direction="row" alignItems="center" gap={1} onClick={stop} flexWrap="wrap">
      <Tooltip title={enabled ? 'Disable this schedule' : 'Enable this schedule'}>
        <span>
          <Switch
            size="small"
            checked={enabled}
            disabled={mutation.isPending}
            onChange={(e) => mutation.mutate({ enabled: e.target.checked, cron: cronDraft, timeZone: timeZoneDraft })}
          />
        </span>
      </Tooltip>
      <TextField
        size="small"
        label="Cron"
        value={cronDraft}
        onChange={(e) => setCronDraft(e.target.value)}
        placeholder="*/5 * * * *"
        error={mutation.isError}
        sx={{ width: 130 }}
        slotProps={{ htmlInput: { style: { fontFamily: 'monospace', fontSize: 13 } } }}
      />
      <TextField
        size="small"
        label="Time zone"
        value={timeZoneDraft}
        onChange={(e) => setTimeZoneDraft(e.target.value)}
        placeholder="UTC"
        sx={{ width: 130 }}
      />
      {dirty && (
        <Tooltip title="Save cron/time zone">
          <span>
            <IconButton
              size="small"
              color="primary"
              disabled={mutation.isPending}
              onClick={() => mutation.mutate({ enabled, cron: cronDraft, timeZone: timeZoneDraft })}
            >
              {mutation.isPending ? <CircularProgress size={16} /> : <SaveIcon fontSize="small" />}
            </IconButton>
          </span>
        </Tooltip>
      )}
      {mutation.isError && (
        <Typography variant="caption" color="error">
          {(mutation.error as Error).message}
        </Typography>
      )}
    </Stack>
  );
}
