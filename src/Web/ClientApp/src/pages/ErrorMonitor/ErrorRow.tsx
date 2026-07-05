import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Stack from '@mui/material/Stack';
import Chip from '@mui/material/Chip';
import Typography from '@mui/material/Typography';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import Collapse from '@mui/material/Collapse';
import Divider from '@mui/material/Divider';
import Switch from '@mui/material/Switch';
import FormControlLabel from '@mui/material/FormControlLabel';
import CircularProgress from '@mui/material/CircularProgress';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { fetchErrorLogDetail } from '../../api/errorLogs';
import type { ErrorLogSummary } from '../../api/types';
import { formatAbsoluteTime, formatRelativeTime } from '../../utils/formatDate';
import { StatusChip } from './StatusChip';
import { useSetErrorResolved } from './useSetErrorResolved';

function DetailField({ label, value }: { label: string; value: string | number | null | undefined }) {
  if (value === null || value === undefined || value === '') return null;
  return (
    <Box sx={{ minWidth: 0 }}>
      <Typography variant="caption" color="text.secondary" display="block">
        {label}
      </Typography>
      <Typography variant="body2" sx={{ wordBreak: 'break-word' }}>
        {value}
      </Typography>
    </Box>
  );
}

function CodeBlock({ label, content }: { label: string; content: string | null | undefined }) {
  if (!content) return null;
  return (
    <Box>
      <Typography variant="caption" color="text.secondary" display="block" sx={{ mb: 0.5 }}>
        {label}
      </Typography>
      <Box
        component="pre"
        sx={{
          m: 0,
          p: 1.5,
          borderRadius: 1.5,
          bgcolor: (theme) => (theme.palette.mode === 'dark' ? 'rgba(255,255,255,0.04)' : 'rgba(0,0,0,0.04)'),
          fontFamily: 'ui-monospace, Consolas, monospace',
          fontSize: 12.5,
          maxHeight: 320,
          overflow: 'auto',
          whiteSpace: 'pre-wrap',
          wordBreak: 'break-word',
        }}
      >
        {content}
      </Box>
    </Box>
  );
}

export function ErrorRow({ error }: { error: ErrorLogSummary }) {
  const [expanded, setExpanded] = useState(false);
  const setResolved = useSetErrorResolved();

  const detailQuery = useQuery({
    queryKey: ['errorLogDetail', error.id],
    queryFn: () => fetchErrorLogDetail(error.id),
    enabled: expanded,
  });

  const detail = detailQuery.data;

  return (
    <Card
      variant="outlined"
      sx={{
        borderLeft: 4,
        borderLeftColor: error.isResolved ? 'success.main' : 'error.main',
        opacity: error.isResolved ? 0.75 : 1,
        transition: 'opacity 0.2s ease',
      }}
    >
      <Box
        onClick={() => setExpanded((v) => !v)}
        sx={{
          p: 1.5,
          cursor: 'pointer',
          display: 'flex',
          flexDirection: 'column',
          gap: 1,
        }}
      >
        <Stack direction="row" alignItems="center" gap={1} flexWrap="wrap">
          <StatusChip isResolved={error.isResolved} />
          <Tooltip title={formatAbsoluteTime(error.createdOn)}>
            <Typography variant="caption" color="text.secondary">
              {formatRelativeTime(error.createdOn)}
            </Typography>
          </Tooltip>
          <Chip size="small" variant="outlined" label={error.exceptionType.split('.').pop()} />
          {error.httpStatusCode && <Chip size="small" variant="outlined" color="warning" label={`HTTP ${error.httpStatusCode}`} />}
          <Box sx={{ flexGrow: 1 }} />
          <FormControlLabel
            onClick={(e) => e.stopPropagation()}
            control={
              <Switch
                size="small"
                checked={error.isResolved}
                onChange={(e) => setResolved.mutate({ id: error.id, resolved: e.target.checked })}
              />
            }
            label={<Typography variant="caption">Resolved</Typography>}
            sx={{ mr: 0 }}
          />
          <IconButton
            size="small"
            sx={{ transform: expanded ? 'rotate(180deg)' : 'none', transition: 'transform 0.15s ease' }}
          >
            <ExpandMoreIcon />
          </IconButton>
        </Stack>

        <Typography
          variant="body2"
          sx={
            expanded
              ? { whiteSpace: 'pre-wrap', wordBreak: 'break-word' }
              : {
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  display: '-webkit-box',
                  WebkitLineClamp: 2,
                  WebkitBoxOrient: 'vertical',
                }
          }
        >
          {error.message}
        </Typography>

        <Stack direction="row" gap={0.75} flexWrap="wrap">
          {error.provider && <Chip size="small" label={`Provider: ${error.provider}`} />}
          {error.country && <Chip size="small" label={`Country: ${error.country}`} />}
          {error.feedOrApiName && <Chip size="small" label={error.feedOrApiName} />}
          <Chip size="small" label={error.source} />
          <Chip size="small" variant="outlined" label={error.applicationName} />
        </Stack>
      </Box>

      <Collapse in={expanded} unmountOnExit>
        <Divider />
        <Box sx={{ p: 2 }} onClick={(e) => e.stopPropagation()}>
          {detailQuery.isLoading && (
            <Stack direction="row" alignItems="center" gap={1}>
              <CircularProgress size={16} />
              <Typography variant="body2" color="text.secondary">
                Loading details...
              </Typography>
            </Stack>
          )}

          {detailQuery.isError && (
            <Typography variant="body2" color="error">
              Failed to load details.
            </Typography>
          )}

          {detail && (
            <Stack gap={2}>
              <Stack direction="row" justifyContent="space-between" alignItems="center" flexWrap="wrap" gap={1}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={detail.isResolved}
                      onChange={(e) => setResolved.mutate({ id: error.id, resolved: e.target.checked })}
                    />
                  }
                  label={detail.isResolved ? 'Marked resolved' : 'Mark as resolved'}
                />
                {detail.resolvedOn && (
                  <Typography variant="caption" color="text.secondary">
                    Resolved {formatAbsoluteTime(detail.resolvedOn)}
                  </Typography>
                )}
              </Stack>

              <Box
                sx={{
                  display: 'grid',
                  gridTemplateColumns: 'repeat(auto-fill, minmax(160px, 1fr))',
                  gap: 1.5,
                }}
              >
                <DetailField label="Exception Type" value={detail.exceptionType} />
                <DetailField label="Error Code" value={detail.errorCode} />
                <DetailField label="HTTP Status" value={detail.httpStatusCode} />
                <DetailField label="Environment" value={detail.environment} />
                <DetailField label="Application" value={detail.applicationName} />
                <DetailField label="Service" value={detail.serviceName} />
                <DetailField label="Machine" value={detail.machineName} />
                <DetailField label="Assembly Version" value={detail.assemblyVersion} />
                <DetailField label="Provider" value={detail.provider} />
                <DetailField label="Feed / API" value={detail.feedOrApiName} />
                <DetailField label="Country" value={detail.country} />
                <DetailField label="Request Path" value={detail.requestPath} />
                <DetailField label="HTTP Method" value={detail.httpMethod} />
                <DetailField label="Trace Id" value={detail.traceId} />
                <DetailField label="Correlation Id" value={detail.correlationId} />
                <DetailField label="Hangfire Job Id" value={detail.hangfireJobId} />
                <DetailField label="IP Address" value={detail.ipAddress} />
                <DetailField label="Execution Duration" value={detail.executionDuration} />
                <DetailField label="Created" value={formatAbsoluteTime(detail.createdOn)} />
              </Box>

              <CodeBlock label="Source URL / Query String" content={[detail.sourceUrl, detail.queryString].filter(Boolean).join('\n')} />
              <CodeBlock label="Inner Exception" content={detail.innerException} />
              <CodeBlock label="Stack Trace" content={detail.stackTrace} />
              <CodeBlock label="Request Body" content={detail.requestBody} />
              <CodeBlock label="Response Body" content={detail.responseBody} />
              <CodeBlock label="Additional Data" content={detail.additionalData} />
            </Stack>
          )}
        </Box>
      </Collapse>
    </Card>
  );
}
