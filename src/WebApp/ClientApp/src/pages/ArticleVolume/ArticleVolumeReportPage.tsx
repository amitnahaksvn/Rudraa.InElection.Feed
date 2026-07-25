import { useState } from 'react';
import Box from '@mui/material/Box';
import Stack from '@mui/material/Stack';
import Tabs from '@mui/material/Tabs';
import Tab from '@mui/material/Tab';
import Typography from '@mui/material/Typography';
import BarChartIcon from '@mui/icons-material/BarChart';
import { ArticleVolumePipelineReport } from './ArticleVolumePipelineReport';

export function ArticleVolumeReportPage() {
  const [tab, setTab] = useState<'Rss' | 'Api'>('Rss');

  return (
    <Box sx={{ maxWidth: 1200, mx: 'auto' }}>
      <Stack direction="row" alignItems="center" gap={1.5} sx={{ mb: 0.5 }}>
        <BarChartIcon color="primary" fontSize="large" />
        <Typography variant="h5" fontWeight={700}>
          Article Volume
        </Typography>
      </Stack>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        How many articles came in per provider - any time, for any date range, straight from actual persisted articles.
      </Typography>

      <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ mb: 2, borderBottom: 1, borderColor: 'divider' }}>
        <Tab label="RSS" value="Rss" />
        <Tab label="APIs" value="Api" />
      </Tabs>

      {/* key={tab} forces a full remount on tab switch - see CrawlReportPage's identical comment;
          the provider filter selection is pipeline-specific and must not carry across tabs. */}
      {tab === 'Rss' ? <ArticleVolumePipelineReport key="Rss" pipeline="Rss" /> : <ArticleVolumePipelineReport key="Api" pipeline="Api" />}
    </Box>
  );
}
